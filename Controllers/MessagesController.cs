using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Controllers.Base;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Repositories.Abstraction;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController: BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto messageDto)
        {
            var userId = User.GetIdentifier();
            
            var currentUser = await _userRepository.GetUserByIdAsync(userId);
            var recipient = await _userRepository.GetUserByNameAsync(messageDto.RecipientUsername);

            if (recipient.Id == userId) return BadRequest("You can't message yourself");

            if (currentUser == null || recipient == null) return NotFound();

            var message = new Message()
            {
                RecipientId = recipient.Id,
                RecipientUsername = recipient.UserName,
                SenderId = currentUser.Id,
                SenderUsername = currentUser.UserName,
                Content = messageDto.Content
            };

            if(message != null) _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetUserMessages([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUsername();
            
            var messages = await _messageRepository.GetUserMessages(messageParams);

            if (messages == null) return BadRequest();
            
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalPages, messages.TotalCount);

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetThread(string username)
        {
            var currentUserName = User.GetUsername();

            var messages = await _messageRepository.GetMessageThread(currentUserName, username);

            if (messages != null) return Ok(messages);

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUserName = User.GetUsername();

            var message = await _messageRepository.GetMessage(id);

            if (message.Recipient.UserName != currentUserName && message.Sender.UserName != currentUserName)
                return Unauthorized();

            if (message.Recipient.UserName == currentUserName) message.RecipientDeleted = true;
            
            if (message.Sender.UserName == currentUserName) message.SenderDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) _messageRepository.RemoveMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Something went wrong");
        }
    }
}