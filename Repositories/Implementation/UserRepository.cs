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
    public class UserRepository: IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(user => user.Photos).ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(user => user.Photos).SingleOrDefaultAsync(user => user.Id == id);
        }

        public async Task<AppUser> GetUserByNameAsync(string name)
        {
            return await _context.Users.Include(user => user.Photos).SingleOrDefaultAsync(user => user.UserName == name);
        }

        public async Task<PagedList<MembersDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.ProjectTo<MembersDto>(_mapper.ConfigurationProvider);

            return await PagedList<MembersDto>.GetPagedList(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<MembersDto> GetMemberByUsernameAsync(string username)
        {
            return await _context.Users.Where(user => user.UserName == username)
                .ProjectTo<MembersDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }
    }
}