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
using API.UnitOfWorks.Abstraction;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoUpload _photoService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoUpload photoUpload)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoUpload;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembersDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await _unitOfWork.UserRepository.GetUserByNameAsync(User.GetUsername());

            userParams.CurrentUsername = user.UserName;

            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalPages, users.TotalCount);
            
            return Ok(users);
        }
        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MembersDto>> GetUser(string username)
        {
            return await _unitOfWork.UserRepository.GetMemberByUsernameAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MembersUpdateDto updateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByNameAsync(User.GetUsername());

            _mapper.Map(updateDto, user);
            
            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }
        
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.UserRepository.GetUserByNameAsync(User.GetUsername());

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

            if (await _unitOfWork.Complete()) 
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("A problem occured");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> UpdateMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByNameAsync(User.GetUsername());

            var photoToMain = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photoToMain.IsMain) return BadRequest("Photo is already main");

            var currentMain = user.Photos.SingleOrDefault(photo => photo.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photoToMain.IsMain = true;
            
            _unitOfWork.UserRepository.Update(user);
            
            if(await _unitOfWork.Complete()) return NoContent();

            return BadRequest();
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByNameAsync(User.GetUsername());

            var photoToDelete = user.Photos.SingleOrDefault(photo => photo.Id == photoId);

            if (photoToDelete == null) return NotFound("Photo does not exist");

            if (photoToDelete.IsMain) return BadRequest("You cannot delete your photo");

            if (photoToDelete.PublicId != null)
            {
                var result = await _photoService.Delete(photoToDelete.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photoToDelete);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest();
        }
    }
}