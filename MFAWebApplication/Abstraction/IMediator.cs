using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;

namespace MFAWebApplication.Abstraction;

public interface IMediator
{
    Task<Result> Send<TCommand>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand;
    Task<Result<TResult>> Send<TCommand, TResult>( TCommand command, CancellationToken cancellationToken )
    where TCommand : ICommand<TResult>;
    Task<Result<TResult>> Query<TQuery, TResult>( TQuery query, CancellationToken cancellationToken )
        where TQuery : IQuery<TResult>;
}