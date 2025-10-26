using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record DeleteUserCommand( Guid UserId ) : ICommand;


internal sealed class DeleteUserCommandHandler
    : ICommandHandler<DeleteUserCommand>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;

    public DeleteUserCommandHandler(UnitOfWork<WriteDbContext> unitOfWork )
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
