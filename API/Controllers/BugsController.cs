using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BugsController(DataContext context) : BaseApiController
    {
        [HttpGet("auth")]
        public ActionResult<string> GetAuth() => "secret text";

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var user = context.Users.Find(-1);
            if (user == null) return NotFound();
            return user;
        }
        
        [HttpGet("server-found")]
        public ActionResult<string> GetServerError()
        {
            var user = context.Users.Find(-1) ?? throw new Exception("Exception happened");
            return "secret text";
        }
        
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Bad request");
        }

    }
}
