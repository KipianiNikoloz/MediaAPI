using API.Abstraction;
using API.Data;
using API.Helpers;
using API.Implementation;
using API.SignalR;
using API.UnitOfWorks.Abstraction;
using API.UnitOfWorks.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<PresenceTracker>();
            
            services.AddScoped<LogUserActivity>();

            services.AddScoped<IPhotoUpload, PhotoUpload>();
            
            services.AddScoped<ITokenService, TokenService>();
            
            services.AddAutoMapper(typeof(AutoMapperProfiles));

            return services;
        }
    }
}