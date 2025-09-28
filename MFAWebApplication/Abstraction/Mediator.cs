using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction;
using System.Reflection;

public class Mediator : IMediator
{
    private readonly Dictionary<Type, Type> _handlers = new();

    public Mediator( Assembly assembly )
    {
        // find all ICommandHandler<T> implementations
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                .Select(i => new { Handler = t, Command = i.GetGenericArguments()[0] }));

        foreach ( var h in handlerTypes )
        {
            _handlers[h.Command] = h.Handler;
        }
    }

    public async Task<Result> Send<TCommand>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand
    {
        if ( !_handlers.TryGetValue(typeof(TCommand), out var handlerType) )
            throw new InvalidOperationException($"No handler found for {typeof(TCommand).Name}");

        // create handler instance (no DI, we use Activator)
        var handler = Activator.CreateInstance(handlerType);

        if ( handler is null )
            throw new InvalidOperationException($"Could not create handler {handlerType.Name}");

        return await ((ICommandHandler<TCommand>) handler).Handle(command, cancellationToken);
    }
}
