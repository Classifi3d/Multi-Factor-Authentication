using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;

namespace MFAWebApplication.Abstraction;

public interface IMediator
{
    Task<Result> Send<TCommand>( TCommand command, CancellationToken cancellationToken )
        where TCommand : ICommand;
}