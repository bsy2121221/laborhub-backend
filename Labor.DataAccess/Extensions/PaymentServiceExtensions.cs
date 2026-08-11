using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.DataAccess.Repositories;
using Labor.DataAccess.Services;
using Labor.DataAccess.Services.Payment;
using Labor.Models.DTOs.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Labor.DataAccess.Extensions;

public static class PaymentServiceExtensions
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentOptions>(configuration.GetSection(PaymentOptions.SectionName));
        services.Configure<RazorpayOptions>(configuration.GetSection(RazorpayOptions.SectionName));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<PaymentCalculator>();
        services.AddScoped<IPaymentService, PaymentService>();

        var provider = configuration["Payment:Provider"] ?? "Mock";
        if (provider.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IPaymentGateway, RazorpayPaymentGateway>();
        }
        else
        {
            services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        }

        return services;
    }
}
