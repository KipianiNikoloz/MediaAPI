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
                .OrderBy(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.UserName && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.Sender.UserName == messageParams.UserName && m.SenderDeleted == false),
                _ => query.Where(m => m.Recipient.UserName == messageParams.UserName && m.MessageRead == null && m.RecipientDeleted == false)
            };

            return await PagedList<MessageDto>.GetPagedList(
                query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider), 
                messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var thread = await _context.Messages
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Where(m => m.Recipient.UserName == 
                            currentUserName && 
                            m.Sender.UserName == recipientUserName && m.RecipientDeleted == false || 
                            m.Recipient.UserName == recipientUserName 
                            && m.Sender.UserName == currentUserName && m.SenderDeleted == false )
                .OrderBy(m => m.MessageSent).
                ToListAsync();

            var unreadMessages = thread
                .Where(m => m.Recipient.UserName == currentUserName && m.MessageRead == null)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                   message.MessageRead = DateTime.Now; 
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(thread);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}