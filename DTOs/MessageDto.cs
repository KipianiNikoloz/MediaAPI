using System;
using API.Entities;

namespace API.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; }
        public string SenderUrl { get; set; }
        public int SenderId { get; set; }
        public string RecipientUsername { get; set; }
        public string RecipientUrl { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime? MessageRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }        
        public bool RecipientDeleted { get; set; }
    }
}