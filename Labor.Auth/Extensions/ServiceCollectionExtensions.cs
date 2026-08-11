using Labor.Auth.IAuthservice;
using Labor.Auth.Authservice;
using Labor.Auth.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Labor.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLaborAuthServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IOTPService, OTPService>();
            
            return services;
        }
    }
} 