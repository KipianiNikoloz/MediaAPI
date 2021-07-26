using System.Threading.Tasks;
using API.Entities;

namespace API.Abstraction
{
    public interface ITokenService
    {
        public Task<string> CreateToken(AppUser user);
    }
}