using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Repositories.Abstraction;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.Implementation
{
    public class MessageRepository: IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(group => group.Connections)
                .FirstOrDefaultAsync(group => group.Name == groupName);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(g => g.Connections)
                .Where(g => g.Connections
                    .Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void RemoveMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(m => m.Recipient)
                .Include(m => m.Sender)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetUserMessages(MessageParams messageParams)
        {
            var query = _context.Messages
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientUsername == messageParams.UserName && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.SenderUsername == messageParams.UserName && m.SenderDeleted == false),
                _ => query.Where(m => m.RecipientUsername == messageParams.UserName && m.MessageRead == null && m.RecipientDeleted == false)
            };

            return await PagedList<MessageDto>.GetPagedList(
                query, 
                messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var thread = await _context.Messages
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .Where(m => m.RecipientUsername == 
                            currentUserName && 
                            m.SenderUsername == recipientUserName && m.RecipientDeleted == false || 
                            m.RecipientUsername == recipientUserName 
                            && m.SenderUsername == currentUserName && m.SenderDeleted == false )
                .OrderBy(m => m.MessageSent).
                ToListAsync();

            var unreadMessages = _context.Messages
                .Where(m => m.RecipientUsername == currentUserName && m.MessageRead == null)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                   message.MessageRead = DateTime.UtcNow; 
                }
            }

            return thread;
        }
    }
}