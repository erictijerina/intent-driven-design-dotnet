using Acme.Application.Contracts;
using Acme.Application.PipelineBehaviors;
using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Application.UseCases.Order.Orchestration;
using Acme.Domain.Abstractions;
using Acme.Infrastructure.BackgroundWorkers.Messaging.Outbox;
using Acme.Infrastructure.Database.Sql;
using Acme.Infrastructure.EventRuntimes;
using Acme.Infrastructure.IntentRuntimes;
using Acme.Infrastructure.Messaging.Email;
using Acme.Infrastructure.Messaging.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Acme.Api.Configuration;

public static class CoreServicesConfigurator
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIntentRuntime, MediatRIntentRuntime>();
        services.AddScoped<IEventRuntime, MediatREventRuntime>();
        services.AddHostedService<OutboxDispatcherBackgroundService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(Program).Assembly,
                typeof(PlaceOrderHandler).Assembly,
                typeof(MediatREventRuntime).Assembly);
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        var connectionString = configuration.GetConnectionString("AcmeContext")
            ?? "Data Source=acme.db";

        services.AddDbContext<AcmeDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IOrderStore, OrderStore>();
        services.AddScoped<EmailSender>();
        services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();
        services.AddScoped<IOutboxDispatcher>(sp => (OutboxMessageStore)sp.GetRequiredService<IOutboxMessageStore>());
        services.AddScoped<IFailedOutboxQuery, FailedOutboxQuery>();

        var mongoConnectionString = configuration.GetConnectionString("AcmeMongo")
            ?? "mongodb://localhost:27017";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("Acme"));

        return services;
    }
}
