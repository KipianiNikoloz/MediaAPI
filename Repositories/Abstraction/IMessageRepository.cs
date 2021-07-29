using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Repositories.Abstraction
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string connectionId);
        void AddMessage(Message message);
        void RemoveMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetUserMessages(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);
    }
}