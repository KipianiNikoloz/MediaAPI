using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Controllers.Base;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Repositories.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController: BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> LikeUser(string username)
        {
            var sourceUserId = User.GetIdentifier();
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
            var likedUser = await _userRepository.GetUserByNameAsync(username);
            
            if (sourceUser == null || likedUser == null) return NotFound();

            if (sourceUserId == likedUser.Id) return BadRequest("You cannot like yourself");

            var like = new UserLike()
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            if (await _likesRepository.GetUserLike(sourceUserId, likedUser.Id) != null)
            {
                sourceUser.LikedUsers.Remove(await _likesRepository.GetUserLike(sourceUserId, likedUser.Id));

                if (await _userRepository.SaveAllAsync()) return Ok();
            }

            sourceUser.LikedUsers.Add(like);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest();
        }

        [HttpGet]
        public async Task<IEnumerable<LikeDto>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetIdentifier();
            var userLikes = await _likesRepository.GetUserLikes(likesParams);
            
            Response.AddPaginationHeader(userLikes.CurrentPage, userLikes.PageSize, userLikes.TotalPages, userLikes.TotalCount);

            return userLikes;
        }
    }
}