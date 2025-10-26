using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.Services;
using OtpNet;
using QRCoder;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record EnableMfaOfUserCommand( Guid userId ) : ICommand<byte[]>;

internal sealed class EnableMfaOfUserCommandHandler : ICommandHandler<EnableMfaOfUserCommand, byte[]>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly ISecurityService _securityService;
    private Guid userId;

    public EnableMfaOfUserCommandHandler( Guid userId )
    {
        this.userId = userId;
    }

    public EnableMfaOfUserCommandHandler( 
        UnitOfWork<WriteDbContext> unitOfWork, 
        ISecurityService securityService )
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
    }

    public async Task<Result<byte[]>> Handle( EnableMfaOfUserCommand request, CancellationToken cancellationToken )
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.userId, cancellationToken);

        if ( user is null )
            return Result.Failure<byte[]>("User not found");

        if ( !string.IsNullOrEmpty(user.MfaSecretKey) )
            return Result.Failure<byte[]>("MFA already enabled for this user");

        // Generate Base32-encoded secret key
        var key = KeyGeneration.GenerateRandomKey(20);
        var secretKey = Base32Encoding.ToString(key);

        // OTP URL
        string issuer = Uri.EscapeDataString("MFA-Security");
        string accountName = Uri.EscapeDataString(user.Email);
        string otpauthUrl = $"otpauth://totp/{issuer}:{accountName}?secret={secretKey}&issuer={issuer}&digits=6";

        // Generate QR code as PNG bytes
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        if(qrBytes is null )
        {
            return Result.Failure<byte[]>("QR Code cannot be generated");
        }

        user.MfaSecretKey = secretKey;
        user.IsMfaEnabled = true;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result.Success(qrBytes);
    }
}


