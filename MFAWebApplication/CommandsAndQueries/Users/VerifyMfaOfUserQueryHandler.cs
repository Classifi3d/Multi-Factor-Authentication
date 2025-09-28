
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.DTOs;
using MFAWebApplication.Services;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record VerifyMfaOfUserQuery( MfaVerificationDTO verificationDto ) : ICommand<string>;

internal sealed class VerifyMfaOfUserQueryHandler : ICommandHandler<VerifyMfaOfUserQuery, string>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly ISecurityService _securityService;
    private readonly IMemoryCache _cache;

    public VerifyMfaOfUserQueryHandler(
        IUnitOfWork unitOfWork,
        ISecurityService securityService,
        IMemoryCache cache
        )
    {
        _unitOfWork = unitOfWork;

        _securityService = securityService;
        _cache = cache;
    }

    public async Task<Result<string>> Handle( VerifyMfaOfUserQuery request, CancellationToken cancellationToken )
    {

        var verification = request.verificationDto;

        if ( !_cache.TryGetValue($"mfa_challenge_{verification.ChallengeId}", out Guid userId) )
        {
            return Result.Failure<string>("Challenge token expired or invalid");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if ( user is null )
            return Result.Failure<string>("User not found");

        // Decode Base32 secret key
        byte[] secretKeyBytes = Google.Authenticator.Base32Encoding.ToBytes(user.MfaSecretKey);

        // Create TOTP instance with a 30-second time step (Google Authenticator standard)
        var totp = new Totp(secretKeyBytes, step: 30);
        // Generate expected OTP for the current time
        var expectedOtp = totp.ComputeTotp();
        Console.WriteLine($"Expected OTP: {expectedOtp}");
        bool isValid = totp.VerifyTotp(verification.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);


        var token = _securityService.CreateToken(user.Id);

        if ( token is null || !isValid )
        {
            return Result.Failure<string>("Invalid MFA code");
        }

        // Remove challenge after successful verification
        _cache.Remove($"mfa_challenge_{verification.ChallengeId}");

        return Result.Success(token);

    }

}
