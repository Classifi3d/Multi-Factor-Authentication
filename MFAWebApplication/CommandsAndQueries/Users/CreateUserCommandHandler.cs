using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.Enteties;
using MFAWebApplication.Kafka;
using MFAWebApplication.Services;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record CreateUserCommand( UserDTO userDto ) : ICommand;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly KafkaProducerService _kafka;
    private readonly ISecurityService _securityService;
    private readonly Mapper _mapper;

    public CreateUserCommandHandler(
        UnitOfWork<WriteDbContext> unitOfWork, 
        KafkaProducerService kafka, 
        Mapper mapper, 
        ISecurityService securityService )
    {
        _unitOfWork = unitOfWork;
        _kafka = kafka;
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

        var userEvent = _mapper.Map<UserCreatedEvent>(user);
        await _kafka.ProduceAsync(userEvent);

        return Result.Success();
    }
}
