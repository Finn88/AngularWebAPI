using API.Dto;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

            var user = mapper.Map<AppUser>(register);
            user.UserName = register.UserName.ToLower();

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
