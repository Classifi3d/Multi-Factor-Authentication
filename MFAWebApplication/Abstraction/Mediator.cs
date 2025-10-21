using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using System.Reflection;

public class Mediator : IMediator
{
    private readonly Dictionary<Type, Type> _handlers = new();

    public Mediator( Assembly assembly )
    {
        // Find all non-abstract, non-interface types
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Select(i => new { Handler = t, Interface = i }));

        foreach ( var h in handlerTypes )
        {
            var def = h.Interface.GetGenericTypeDefinition();

            if ( def == typeof(ICommandHandler<>)
             || def == typeof(ICommandHandler<,>)
             || def == typeof(IQueryHandler<,>) )
            {
                var requestType = h.Interface.GetGenericArguments()[0];
                _handlers[requestType] = h.Handler;
            }
        }
    }

    public async Task<Result> Send<TCommand>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand
    {
        if ( !_handlers.TryGetValue(typeof(TCommand), out var handlerType) )
            throw new InvalidOperationException($"No handler found for {typeof(TCommand).Name}");

        var handler = Activator.CreateInstance(handlerType)
                      ?? throw new InvalidOperationException($"Could not create handler {handlerType.Name}");

        return await ((ICommandHandler<TCommand>) handler).Handle(command, cancellationToken);
    }

    public async Task<Result<TResult>> Send<TCommand, TResult>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand<TResult>
    {
        if ( !_handlers.TryGetValue(typeof(TCommand), out var handlerType) )
            throw new InvalidOperationException($"No handler found for {typeof(TCommand).Name}");

        var handler = Activator.CreateInstance(handlerType)
                      ?? throw new InvalidOperationException($"Could not create handler {handlerType.Name}");

        return await ((ICommandHandler<TCommand, TResult>) handler).Handle(command, cancellationToken);
    }

    public async Task<Result<TResult>> Query<TQuery, TResult>( TQuery query, CancellationToken cancellationToken )
        where TQuery : IQuery<TResult>
    {
        if ( !_handlers.TryGetValue(typeof(TQuery), out var handlerType) )
            throw new InvalidOperationException($"No handler found for {typeof(TQuery).Name}");

        var handler = Activator.CreateInstance(handlerType)
                      ?? throw new InvalidOperationException($"Could not create handler {handlerType.Name}");

        return await ((IQueryHandler<TQuery, TResult>) handler).Handle(query, cancellationToken);
    }
}
