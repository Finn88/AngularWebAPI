using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimExtension
    {
        public static string? GetUserName(this ClaimsPrincipal claim)
        {
            var userName = claim.FindFirst(ClaimTypes.Name)?.Value
                ?? throw new Exception("User not found in token");
            return userName; 
        }
        public static int GetUserId(this ClaimsPrincipal claim)
        {
            var userId = claim.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User not found in token");
            return int.Parse(userId); 
        }
    }
}
