using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Controllers.Base;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Repositories.Abstraction;
using API.UnitOfWorks.Abstraction;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController: BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetUserMessages([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUsername();
            
            var messages = await _unitOfWork.MessageRepository.GetUserMessages(messageParams);

            if (messages == null) return BadRequest();
            
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalPages, messages.TotalCount);

            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUserName = User.GetUsername();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.Recipient.UserName != currentUserName && message.Sender.UserName != currentUserName)
                return Unauthorized();

            if (message.Recipient.UserName == currentUserName) message.RecipientDeleted = true;
            
            if (message.Sender.UserName == currentUserName) message.SenderDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) _unitOfWork.MessageRepository.RemoveMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Something went wrong");
        }
    }
}