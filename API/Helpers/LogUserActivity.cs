using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultCcontext = await next();

            if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

            var userId = resultCcontext.HttpContext.User.GetUserId();
            var repository = resultCcontext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var user = await repository.UserRepository.GetUserByIdAsync(userId);
            if(user == null) return;

            user.LastActive = DateTime.UtcNow;
            await repository.Complete();
        }
    }
}
