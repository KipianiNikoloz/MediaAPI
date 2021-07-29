using System;
using System.Threading.Tasks;
using API.Extensions;
using API.UnitOfWorks.Abstraction;
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
            var unitOfWork = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();

            var user = await unitOfWork.UserRepository.GetUserByIdAsync(id);
            user.LastActive = DateTime.UtcNow;
            await unitOfWork.Complete();
        }
    }
}