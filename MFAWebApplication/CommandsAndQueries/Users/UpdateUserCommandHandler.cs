using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Repository;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record UpdateUserCommand( UserDTO User ) : ICommand;


internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler( IUserRepository userRepository )
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle( UpdateUserCommand request, CancellationToken cancellationToken )
    {
        var isUpdated = true;//await _userRepository.Update(request.User);

        return isUpdated
            ? Result.Success()
            : Result.Failure("User update failed.");
    }
}
