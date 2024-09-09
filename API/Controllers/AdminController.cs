using API.Data;
using API.Dto;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  
  public class AdminController(UserManager<AppUser> usersManager, IUnitOfWork unitOfWork) : BaseApiController
  {

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
      var users = await usersManager
        .Users
        .OrderBy(x => x.UserName)
        .Select(x => new
        {
          x.Id,
          UserName = x.UserName,
          Roles = x.UserRoles.Select(r => r.Role.Name).ToList(),
        })
        .ToListAsync();

      return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string userName, string roles)
    {
      if (string.IsNullOrEmpty(roles)) return BadRequest();

      var selectRoles = roles.Split(',').ToArray();

      var user = await usersManager.FindByNameAsync(userName);
      if (user is null) return BadRequest();

      var userRoles = await usersManager.GetRolesAsync(user);

      var result = await usersManager.AddToRolesAsync(user, selectRoles.Except(userRoles));
      if (!result.Succeeded) return BadRequest();

      result = await usersManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));
      if (!result.Succeeded) return BadRequest();

      return Ok(await usersManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
      return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPut("confirm-photo/{photoId:int}")]
    public async Task<ActionResult> ConfirmPhoto(int photoId)
    {
      if (await unitOfWork.PhotoRepository.ConfirmPhoto(photoId) == false)
        return BadRequest("Photo was not found");

      if (await unitOfWork.Complete()) return Ok();

      return BadRequest("Photo was not confirmed");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-for-confirm")]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosForConform()
    {
      var result = await unitOfWork.PhotoRepository.GetPhotosForConfirm();
      return Ok(result);
    }
  }
}
