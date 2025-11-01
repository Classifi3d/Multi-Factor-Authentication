using AuthenticationWebApplication.DTOs;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record UpdateUserCommand( UserDTO User ) : ICommand;


internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;

    public UpdateUserCommandHandler( UnitOfWork<WriteDbContext> unitOfWork )
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle( UpdateUserCommand request, CancellationToken cancellationToken )
    {
        var isUpdated = true;//await _userRepository.Update(request.User);

        return isUpdated
            ? Result.Success()
            : Result.Failure("User update failed.");
    }
}
