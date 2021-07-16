using System;
using System.Threading.Tasks;
using API.Extensions;
using API.Repositories.Abstraction;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity: IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var id = resultContext.HttpContext.User.GetIdentifier();
            var _repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();

            var user = await _repo.GetUserByIdAsync(id);
            user.LastActive = DateTime.Now;
            await _repo.SaveAllAsync();
        }
    }
}