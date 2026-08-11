using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.DataAccess.Repositories;
using Labor.DataAccess.Services;
using Labor.DataAccess.Services.Telephony;
using Labor.Models.DTOs.TelePhony;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Labor.DataAccess.Extensions;

public static class TelephonyServiceExtensions
{
    public static IServiceCollection AddTelephonyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelephonyOptions>(configuration.GetSection(TelephonyOptions.SectionName));
        services.Configure<ExotelOptions>(configuration.GetSection(ExotelOptions.SectionName));

        services.AddScoped<ILaborConfirmationRepository, LaborConfirmationRepository>();
        services.AddScoped<ILaborConfirmationService, LaborConfirmationService>();
        services.AddScoped<INotificationService, MockNotificationService>();

        var provider = configuration["Telephony:Provider"] ?? "Mock";
        if (provider.Equals("Exotel", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IIvrService, ExotelIvrService>();
        }
        else
        {
            services.AddScoped<IIvrService, MockIvrService>();
        }

        return services;
    }
}
