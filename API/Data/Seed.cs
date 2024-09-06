using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager) {
            if (await userManager.Users.AnyAsync()) return;

            var usersData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<AppUser>>(usersData, options);
            if (users is null) return;


            var roles = new List<AppRole> 
            { 
                new() { Name = "Member" } ,
                new() { Name = "Admin" } ,
                new() { Name = "Moderator" } 
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users) {
                if (user is null) continue; 

              //  using var hmac = new HMACSHA512();
              //  user.UserName = user.UserName?.ToLower();
              //  user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("1"));
               // user.PasswordSalt = hmac.Key;
               // context.Users.Add(user);

                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }
            var admin = new AppUser
            {
                UserName = "admin",
                City = string.Empty,
                Country = string.Empty,
                Gender = string.Empty,
                KnownAs = string.Empty
            };
            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRoleAsync(admin, "Admin");

            //await context.SaveChangesAsync();
        }
    }
}
