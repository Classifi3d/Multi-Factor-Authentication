using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.DTOs;
using MFAWebApplication.Enteties;
using MFAWebApplication.Kafka;
using MFAWebApplication.Projections;
using MFAWebApplication.Services;
using System.Reflection;
namespace MFAWebApplication.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices( this IServiceCollection services )
    {

        // Infrastructure
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddScoped<UnitOfWork<WriteDbContext>>();
        //services.AddScoped<UnitOfWork<ReadDbContext>>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadModelRepository<>), typeof(ReadModelRepository<>));

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
        );

        services.AddScoped<IMediator>(sp => new Mediator(Assembly.GetExecutingAssembly(), sp));

        // Services
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddAutoMapper(assemblies);
        services.AddSingleton(MapperConfiguration.InitializeAutomapper());
        services.AddMemoryCache();

        // Messaging Queue
        services.AddSingleton<KafkaProducerService>();
        services.AddHostedService<KafkaConsumerService>();
        services.AddHostedService(provider => provider.GetRequiredService<KafkaConsumerService>());

        services.AddScoped<UserCreatedProjector>();

        var projectorMap = new Dictionary<string, Type>
        {
            [nameof(UserCreatedEvent)] = typeof(UserCreatedProjector),
            // [OrderCreatedEvent)] = typeof(OrderCreatedProjector)
        };
        services.AddSingleton<IDictionary<string, Type>>(projectorMap);


        // Doesn't stop the program in case of background events crashing
        services.Configure<HostOptions>(opts =>
        {
            opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });


        return services;
    }
}
