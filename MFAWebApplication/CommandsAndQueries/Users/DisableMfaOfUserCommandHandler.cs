using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record DisableMfaOfUserCommand( Guid userId ) : ICommand;

internal sealed class DisableMfaOfUserCommandHandler : ICommandHandler<DisableMfaOfUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;


    public DisableMfaOfUserCommandHandler(
        IUnitOfWork unitOfWork )
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle( DisableMfaOfUserCommand request, CancellationToken cancellationToken )
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.userId, cancellationToken);

        if ( user is null )
            return Result.Failure("User not found");

        if ( !string.IsNullOrEmpty(user.MfaSecretKey) )
            return Result.Failure("MFA is already disabled for this user");

        user.MfaSecretKey = null;
        user.IsMfaEnabled = true;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }

}
