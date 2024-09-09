using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignaR;
using Microsoft.EntityFrameworkCore;

namespace API.Extentions
{
  public static class ServiceExtensions
  {
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddControllers();
      services.AddDbContext<DataContext>(options =>
      {
        options.UseSqlServer(config.GetConnectionString("DefaultConnection"));

      });

      services.AddCors();
      services.AddScoped<ITokenService, TokenService>();
      services.AddScoped<IUserRepository, UserRepository>();
      services.AddScoped<ILikesRepository, LikesRepository>();
      services.AddScoped<IMessageRepository, MessageRepository>();
      services.AddScoped<IPhotoRepository, PhotoRepository>();
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
      services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
      services.AddScoped<IPhotoService, PhotoService>();
      services.AddScoped<LogUserActivity>();
      services.AddSignalR();
      services.AddSingleton<PresenceTracker>();
      return services;
    }
  }
}
