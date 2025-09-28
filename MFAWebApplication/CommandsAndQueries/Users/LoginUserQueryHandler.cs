using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Repository;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.DTOs;
using MFAWebApplication.Services;
using Microsoft.Extensions.Caching.Memory;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record LoginUserQuery( UserLoginDTO userLoginDto ) : IQuery<LoginSecurityDTO>;

internal sealed class LoginUserQueryHandler : IQueryHandler<LoginUserQuery, LoginSecurityDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityService _securityService;
    private readonly IMemoryCache _cache;

    public LoginUserQueryHandler(
        IUserRepository userRepository,
        ISecurityService securityService,
        IMemoryCache cache )

    {
        _userRepository = userRepository;
        _securityService = securityService;
        _cache = cache;
    }

    public async Task<Result<LoginSecurityDTO>> Handle( LoginUserQuery request, CancellationToken cancellationToken )
    {
        var loginDto = request.userLoginDto;
        var userEmail = loginDto.Email;

        var user = await _userRepository.GetByEmailAsync(userEmail, cancellationToken);
        if ( user == null )
        {
            return Result.Failure<LoginSecurityDTO>("Invalid credentials");
        }


        var hashedPassword = _securityService.PasswordHashing(loginDto.Password);
        if ( hashedPassword != user.Password )
        {
            return Result.Failure<LoginSecurityDTO>("Invalid credentials");
        }

        if ( !user.IsMfaEnabled )
        {
            var token = _securityService.CreateToken(user.Id);
            var result = new LoginSecurityDTO
            {
                Token = token,
                RequiresMfa = false,
                ChallengeId = null
            };

            return Result.Success(result);
        }

        var challengeId = Guid.NewGuid().ToString();
        _cache.Set($"mfa_challenge_{challengeId}", user.Id, TimeSpan.FromMinutes(5));

        var mfaResult = new LoginSecurityDTO
        {
            Token = null,
            RequiresMfa = true,
            ChallengeId = challengeId
        };

        return Result.Success(mfaResult);

    }

}