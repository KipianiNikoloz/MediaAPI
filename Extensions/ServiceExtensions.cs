using API.Abstraction;
using API.Data;
using API.Helpers;
using API.Implementation;
using API.Repositories.Abstraction;
using API.Repositories.Implementation;
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

            services.AddScoped<LogUserActivity>();
            
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<ILikesRepository, LikesRepository>();
            
            services.AddScoped<IPhotoUpload, PhotoUpload>();
            
            services.AddAutoMapper(typeof(AutoMapperProfiles));

            return services;
        }
    }
}