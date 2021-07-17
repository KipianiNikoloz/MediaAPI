using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.Implementation
{
    public class LikesRepository: ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        
        public async Task<UserLike> GetUserLike(int sourceId, int likedId)
        {
            return await _context.UserLikes.FindAsync(sourceId, likedId);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(user => user.LikedUsers)
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(user => user.UserName).AsQueryable();
            var likes = _context.UserLikes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(likes => likes.SourceUserId == likesParams.UserId);
                users = likes.Select(likes => likes.LikedUser);
            }
            
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(likes => likes.LikedUserId == likesParams.UserId);
                users = likes.Select(likes => likes.SourceUser);
            }

            var likeDtos = users.Select(user => new LikeDto()
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                City = user.City,
                PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain).Url,
                Id = user.Id
            });

            return await PagedList<LikeDto>.GetPagedList(likeDtos, likesParams.PageNumber, likesParams.PageSize);
        }
    }
}