using AuthenticationWebApplication.Context;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Repository;
using MFAWebApplication.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
namespace MFAWebApplication.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices( this IServiceCollection services )
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

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


        //services.AddScoped<IMediator, Mediator>();

        //var assembly = Assembly.GetExecutingAssembly();

        //var handlerTypes = assembly.GetTypes()
        //    .Where(t => !t.IsAbstract && !t.IsInterface)
        //    .SelectMany(t => t.GetInterfaces()
        //        .Where(i => i.IsGenericType &&
        //            (
        //                i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
        //                i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
        //                i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
        //            ),
        //        ( impl, iface ) => new { Impl = impl, Interface = iface }));

        //foreach ( var handler in handlerTypes )
        //{
        //    services.AddScoped(handler.Interface, handler.Impl);
        //    services.AddScoped(handler.Impl); // 👈 Needed for Mediator .AsSelf() resolution
        //}



        return services;
    }
}
