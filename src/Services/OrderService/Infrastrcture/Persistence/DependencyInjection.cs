using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstraction;
using OrderService.Domain.Orders;
using OrderService.Infrastrcture.Persistence.Repositories;

namespace OrderService.Infrastrcture.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("OrderDbConnection"));
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}