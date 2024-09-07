using API.Dto;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToogleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            if (sourceUserId == targetUserId) return BadRequest("Cant like yourself");

            var existingLike = await unitOfWork.LikeRepository.GetUserLike(sourceUserId, targetUserId);

            if (existingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };
                unitOfWork.LikeRepository.AddLike(like);
            }
            else
            {
                unitOfWork.LikeRepository.DeleteLike(existingLike);
            }

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to update like");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds() 
        {
            var sourceUserId = User.GetUserId();
            return Ok(await unitOfWork.LikeRepository.GetCurrentUserLikeIds(sourceUserId));
        }

        [HttpGet()]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams) 
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikeRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
    }
}
