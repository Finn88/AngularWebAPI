using API.Dto;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize()]
    public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUsername = User.GetUserName();
            var user = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(user);
            return Ok(user);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) 
        {
            var user = await unitOfWork.UserRepository.GetMemberByNameAsync(username);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userName == null) return BadRequest("User not found in token");

            var user = await unitOfWork.UserRepository.GetUserByNameAsync(userName);
            if (userName == null) return BadRequest("User not found in Db");

            mapper.Map(memberUpdateDto, user);

            if(await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var userName = User.GetUserName();
            var user = await unitOfWork.UserRepository.GetUserByNameAsync(userName);
            if (user == null) return BadRequest("User not found in Db");

            var result = await photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (!user.Photos.Any())
                photo.IsMain = true;

            user.Photos.Add(photo);

            if (await unitOfWork.Complete())
                return CreatedAtAction(
                    nameof(GetUser),
                    new { userName = user.UserName },
                    mapper.Map<PhotoDto>(photo));

            return BadRequest("Unable to add photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var userName = User.GetUserName();
            var user = await unitOfWork.UserRepository.GetUserByNameAsync(userName);
            if (user == null) return BadRequest("User not found in Db");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("Can't use as the main photo");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Unable to set main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var userName = User.GetUserName();
            var user = await unitOfWork.UserRepository.GetUserByNameAsync(userName);
            if (user == null) return BadRequest("User not found in Db");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("Can't delete photo");

            var currentMain = user.Photos.Remove(photo);

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error);
            }
            user.Photos.Remove(photo);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Unable to delete photo");
        }
    }
}
