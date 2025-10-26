using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;

namespace MFAWebApplication.CommandsAndQueries.Users;


public sealed record GetUserProfileQuery( Guid UserId ) : IQuery<User>;

internal sealed class GetUserProfileQueryHandler
    : IQueryHandler<GetUserProfileQuery, User>
{
    private readonly IReadUnitOfWork _unitOfWork;


    public GetUserProfileQueryHandler(IReadUnitOfWork unitOfWork )
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<User>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken )
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken);

        if ( user is null )
            return Result.Failure<User>("User not found");


        return Result.Success(user);
    }
}
