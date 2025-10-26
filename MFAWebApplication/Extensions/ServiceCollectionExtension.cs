using AuthenticationWebApplication.Context;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Services;
using System.Reflection;
namespace MFAWebApplication.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices( this IServiceCollection services )
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReadUnitOfWork, ReadUnitOfWork>();

        services.AddScoped<ISecurityService, SecurityService>();
        services.AddSingleton(MapperConfiguration.InitializeAutomapper());
        services.AddMemoryCache();

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

        return services;
    }
}
