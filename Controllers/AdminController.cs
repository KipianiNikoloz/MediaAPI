using System.Linq;
using System.Threading.Tasks;
using API.Controllers.Base;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController: BaseController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithPolicy()
        {
            var users = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(u => u.Role)
                .OrderBy(u => u.UserName)
                .Select( u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string userRoles)
        {
            var selectedRoles = userRoles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(roles));

            if (!result.Succeeded) return BadRequest("Failed to add user to roles");

            result = await _userManager.RemoveFromRolesAsync(user, roles.Except(selectedRoles));
            
            if (!result.Succeeded) return BadRequest("Failed to remove user from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderation")]
        public ActionResult GetPhotoModeration()
        {
            return Ok("Only moderators can see this");
        }
    }
}