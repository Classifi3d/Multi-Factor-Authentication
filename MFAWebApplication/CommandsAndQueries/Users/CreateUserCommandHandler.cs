using AuthenticationWebApplication.Context;
using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AuthenticationWebApplication.Repository;
using AutoMapper;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Services;
using Microsoft.Extensions.Caching.Memory;

namespace MFAWebApplication.CommandsAndQueries.Users;


public sealed record CreateUserCommand( UserDTO userDto ) : ICommand;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly Mapper _mapper;
    private readonly SecurityService _securityService;

    public CreateUserCommandHandler( IUnitOfWork unitOfWork, IUserRepository userRepository, Mapper mapper, SecurityService securityService )
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
        _securityService = securityService;
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

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result.Success();
    }
}
