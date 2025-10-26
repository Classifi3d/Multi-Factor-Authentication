using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record DeleteUserCommand( Guid UserId ) : ICommand;


internal sealed class DeleteUserCommandHandler
    : ICommandHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler( IUnitOfWork unitOfWork )
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle( DeleteUserCommand request, CancellationToken cancellationToken )
    {
        var isDeleted = true; //await _userRepository.DeleteUserAsync(request.UserId, cancellationToken);

        return isDeleted
            ? Result.Success()
            : Result.Failure("User not found.");
    }
}
