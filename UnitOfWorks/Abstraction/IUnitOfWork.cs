using System.Threading.Tasks;
using API.Repositories.Abstraction;

namespace API.UnitOfWorks.Abstraction
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILikesRepository LikesRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}