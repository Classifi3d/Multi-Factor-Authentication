using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;

namespace MFAWebApplication.CommandsAndQueries.Users;


public sealed record GetUserProfileQuery( Guid UserId ) : IQuery<User>;

internal sealed class GetUserProfileQueryHandler
    : IQueryHandler<GetUserProfileQuery, User>
{
    private readonly IUnitOfWork _unitOfWork;


    public GetUserProfileQueryHandler( IUnitOfWork unitOfWork )
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<User>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken )
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if ( user is null )
            return Result.Failure<User>("User not found");


        return Result.Success(user);
    }
}
