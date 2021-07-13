using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Abstraction;
using API.Controllers.Base;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Repositories.Abstraction;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController: BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoUpload _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoUpload photoUpload)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoUpload;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembersDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var users = await _userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalPages, users.TotalCount);
            
            return Ok(users);
        }
        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MembersDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberByUsernameAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MembersUpdateDto updateDto)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            _mapper.Map(updateDto, user);
            
            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
        
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            var result = await _photoService.Upload(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo()
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            
            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync()) 
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("A problem occured");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> UpdateMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            var photoToMain = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photoToMain.IsMain) return BadRequest("Photo is already main");

            var currentMain = user.Photos.SingleOrDefault(photo => photo.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photoToMain.IsMain = true;
            
            _userRepository.Update(user);
            
            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest();
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            var photoToDelete = user.Photos.SingleOrDefault(photo => photo.Id == photoId);

            if (photoToDelete == null) return NotFound("Photo does not exist");

            if (photoToDelete.IsMain) return BadRequest("You cannot delete your photo");

            if (photoToDelete.PublicId != null)
            {
                var result = await _photoService.Delete(photoToDelete.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photoToDelete);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest();
        }
    }
}