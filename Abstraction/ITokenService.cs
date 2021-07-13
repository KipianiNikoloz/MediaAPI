using API.Entities;

namespace API.Abstraction
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}