using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Repositories.Abstraction;
using API.UnitOfWorks.Abstraction;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Group = API.Entities.Group;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub: Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;

        public MessageHub(IUnitOfWork unitOfWork ,IMapper mapper, 
            IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            
            var user = Context.User.GetUsername();
            var otherUser = httpContext.Request.Query["user"].ToString();

            var messageGroupName = GetMessageGroup(user, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, messageGroupName);
            await AddToGroup(messageGroupName);

            var thread = await _unitOfWork.MessageRepository
                .GetMessageThread(user, otherUser);

            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();
            
            await Clients.Group(messageGroupName).SendAsync("ReceiveMessageThread", thread);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto messageDto)
        {
            var userId = Context.User.GetIdentifier();
            
            var currentUser = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
            var recipient = await _unitOfWork.UserRepository.GetUserByNameAsync(messageDto.RecipientUsername);

            if (recipient.Id == userId) throw new HubException("You cannot send messages to yourself");

            if (currentUser == null || recipient == null) throw new HubException("Not found");

            var message = new Message()
            {
                RecipientId = recipient.Id,
                RecipientUsername = recipient.UserName,
                SenderId = currentUser.Id,
                SenderUsername = currentUser.UserName,
                Content = messageDto.Content
            };
            
            var groupName = GetMessageGroup(currentUser.UserName, recipient.UserName);

            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            message.MessageSent = message.MessageSent.SetKindUtc();
            
            if (group.Connections.Any(connection => connection.UserName == recipient.UserName))
            {
                message.MessageRead = DateTime.UtcNow;
                message.MessageRead = message.MessageRead.SetKindUtc();
            }
            else
            {
                var connections = await _presenceTracker.GetConnections(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("InformUserOfMessage", new
                    {
                        currentUser.UserName,
                        currentUser.KnownAs
                    });
                }
            }

            if(message != null) _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
            };
            
        }

        private async Task AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            
            group.Connections.Add(connection);

            await _unitOfWork.Complete();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            await _unitOfWork.Complete();
        }

        private string GetMessageGroup(string caller, string other)
        {
            var stringMatch = string.CompareOrdinal(caller, other);

            return stringMatch < 0 ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}