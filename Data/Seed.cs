using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedData(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>()
            {
                new AppRole {Name = "Member"},
                new AppRole {Name = "Moderator"},
                new AppRole {Name = "Admin"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            
            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                user.DateOfBirth = user.DateOfBirth.SetKindUtc();

                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser()
            {
                UserName = "admin"
            };

            admin.DateOfBirth = admin.DateOfBirth.SetKindUtc();
            admin.Created = admin.Created.SetKindUtc();
            admin.LastActive = admin.LastActive.SetKindUtc();

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
        
    }
}