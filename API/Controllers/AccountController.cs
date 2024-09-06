using API.Dto;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace API.Controllers
{
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, 
        IMapper mapper) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {
            var userInDb = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == register.UserName);
            if (userInDb is not null) return BadRequest("User exists");

          //  using var hmac = new HMACSHA512();

            var user = mapper.Map<AppUser>(register);
            user.UserName = register.UserName.ToLower();
           // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password));
           // user.PasswordSalt = hmac.Key;
            

          //  context.Users.Add(user);
           // await context.SaveChangesAsync();


            var result = await userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded) return Unauthorized("No user found");

            if (user is null) return Unauthorized("No user found");
            return new UserDto
            {
                Token = await tokenService.CreateToken(user),
                UserName = user?.UserName!,
                KnownAs = user?.KnownAs!,
                Gender = user?.Gender!
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login) 
        {
            var user = await userManager.Users.Include(u => u.Photos)
                .FirstOrDefaultAsync(x => x.NormalizedUserName == login.UserName.ToUpper());
            
            if(user is null) return Unauthorized("No user found");

            //   using var hmac = new HMACSHA512(user.PasswordSalt);
            //    var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
            //   if(!user.PasswordHash.SequenceEqual(passwordHash)) return Unauthorized("Invalid password");

            var result = await userManager.CheckPasswordAsync(user, login.Password);
            if (!result) return Unauthorized("Invalid password");

            return new UserDto
            {
                Token = await tokenService.CreateToken(user),
                UserName = user.UserName!,
                KnownAs = user.KnownAs!,
                Gender = user.Gender!,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
            };
        }
    }
}
