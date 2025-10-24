using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MFAWebApplication.Abstraction;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _commandHandlers = new();
    private readonly Dictionary<Type, Type> _commandWithResultHandlers = new();
    private readonly Dictionary<Type, Type> _queryHandlers = new();

    public Mediator(Assembly assembly, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    var definition = i.GetGenericTypeDefinition();
                    var args = i.GetGenericArguments();

                    if (definition == typeof(ICommandHandler<>))
                        _commandHandlers[args[0]] = type;

                    else if (definition == typeof(ICommandHandler<,>))
                        _commandWithResultHandlers[args[0]] = type;

                    else if (definition == typeof(IQueryHandler<,>))
                        _queryHandlers[args[0]] = type;
                }
            }
        }
    }

    public async Task<Result> Send<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        if (!_commandHandlers.TryGetValue(typeof(TCommand), out var handlerType))
            throw new InvalidOperationException($"No handler found for {typeof(TCommand).Name}");

        var handler = _serviceProvider.GetRequiredService(handlerType);
        return await ((ICommandHandler<TCommand>)handler).Handle(command, cancellationToken);
    }

    public async Task<Result<TResult>> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResult>
    {
        if (!_commandWithResultHandlers.TryGetValue(typeof(TCommand), out var handlerType))
            throw new InvalidOperationException($"No handler found for {typeof(TCommand).Name}");

        var handler = _serviceProvider.GetRequiredService(handlerType);
        return await ((ICommandHandler<TCommand, TResult>)handler).Handle(command, cancellationToken);
    }

    public async Task<Result<TResult>> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
    {
        if (!_queryHandlers.TryGetValue(typeof(TQuery), out var handlerType))
            throw new InvalidOperationException($"No handler found for {typeof(TQuery).Name}");

        var handler = _serviceProvider.GetRequiredService(handlerType);
        return await ((IQueryHandler<TQuery, TResult>)handler).Handle(query, cancellationToken);
    }
}
