using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstraction;
using OrderService.Domain.Orders;
using OrderService.Infrastrcture.Persistence;
using OrderService.Infrastrcture.Persistence.Repositories;
using OrderService.Infrastructure.Messaging;

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


        services.AddMassTransit(x =>
        {
            var rabbitConnectionString = configuration.GetConnectionString("rabbitmq");

            x.UsingRabbitMq((context, cfg) =>
            {
                if (!string.IsNullOrEmpty(rabbitConnectionString))
                {
                    cfg.Host(new Uri(rabbitConnectionString));
                }

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<RabbitMqOutboxPublisher>();

        return services;
    }
}