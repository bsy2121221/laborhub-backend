using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.DataAccess.Repositories;
using Labor.DataAccess.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Labor.DataAccess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services)
        {
            // Context
            services.AddScoped<IDbContext, DapperContext>();
            
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOTPRepository, OTPRepository>();
            services.AddScoped<ILaborRepository, LaborRepository>();
            services.AddScoped<ILaborService, LaborService>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IAdminRoleService, AdminRoleService>();
            services.AddScoped<IAdminManagementRepository, AdminManagementRepository>();
            services.AddScoped<IAdminManagementService, AdminManagementService>();

            
            return services;
        }
    }
} 