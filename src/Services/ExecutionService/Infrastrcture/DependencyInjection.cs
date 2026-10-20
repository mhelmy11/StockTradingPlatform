using System;
using ExecutionService.Domain.Executions;
using ExecutionService.Infrastrcture.Messaging.Consumers;
using ExecutionService.Infrastrcture.Persistence;
using ExecutionService.Infrastrcture.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace ExecutionService.Infrastrcture;

public static class DependencyInjection
{
    public static IServiceCollection AddExecutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ExecutionDbConnection")
            ?? throw new InvalidOperationException("Connection string 'ExecutionDbConnection' not found.");

        services.AddDbContext<ExecutionDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IExecutionRepository, ExecutionRepository>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ExecuteOrderConsumer>();

            var rabbitConnectionString = configuration.GetConnectionString("rabbitmq");

            x.UsingRabbitMq((context, cfg) =>
            {
                if (!string.IsNullOrEmpty(rabbitConnectionString))
                    cfg.Host(new Uri(rabbitConnectionString));

                cfg.ReceiveEndpoint("execution-service-execute-order", e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(2),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(2)));

                    e.ConfigureConsumer<ExecuteOrderConsumer>(context);
                });
            });
        });

        return services;
    }
}