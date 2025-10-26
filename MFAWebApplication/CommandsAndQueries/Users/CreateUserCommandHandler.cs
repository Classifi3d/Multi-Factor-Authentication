using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Services;

namespace MFAWebApplication.CommandsAndQueries.Users;


public sealed record CreateUserCommand( UserDTO userDto ) : ICommand;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityService _securityService;
    private readonly Mapper _mapper;

    public CreateUserCommandHandler( IUnitOfWork unitOfWork, Mapper mapper, ISecurityService securityService )
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
        _mapper = mapper;
    }

    public async Task<Result> Handle( CreateUserCommand request, CancellationToken cancellationToken )
    {
        var user = _mapper.Map<User>(request.userDto);
        if ( user is null )
        {
            return Result.Failure("Creating user failed");

        }
        user.Id = Guid.NewGuid();
        user.Password = _securityService.PasswordHashing(request.userDto.Password);
        user.CreateDate = DateTime.UtcNow;
        user.UpdateDate = DateTime.UtcNow;

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result.Success();
    }
}
