using AuthenticationWebApplication.Context;
using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using MFAWebApplication.DTOs;
using MFAWebApplication.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;
using QRCoder;

namespace AuthenticationWebApplication.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly Mapper _mapper;
    private readonly SecurityService _securityService;

    public UserRepository( ApplicationDbContext dbContext,
                        IConfiguration configuration,
                        IMemoryCache cache,
                        SecurityService securityService )
    {
        _dbContext = dbContext;
        _mapper = Context.MapperConfiguration.InitializeAutomapper();
        _cache = cache;
        _securityService = securityService;
    }

    public async Task<IEnumerable<UserDTO>?> GetUsersAsync()
    {
        var users = await _dbContext.User.ToListAsync();
        if ( !users.Any() == false )
        {
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }
        else
        {
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default )
    {
        return await _dbContext.User
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default )
    {
        return await _dbContext.User
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AddAsync( User user, CancellationToken cancellationToken = default )
    {
        await _dbContext.User.AddAsync(user, cancellationToken);
    }

    public void Update( User user )
    {
        _dbContext.User.Update(user);
    }

    public void Delete( User user )
    {
        _dbContext.User.Remove(user);
    }



    public async Task<bool> AddUserAsync( UserDTO userDTO )
    {
        var user = _mapper.Map<User>(userDTO);
        if ( user != null )
        {
            user.Id = Guid.NewGuid();
            user.Password = _securityService.PasswordHashing(userDTO.Password);
            await _dbContext.User.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync( UserDTO userDTO )
    {
        var user = _dbContext.User.Where(x => x.Email == userDTO.Email).FirstOrDefault();
        if ( user != null )
        {
            user.Email = userDTO.Email;
            user.Username = userDTO.Username;
            if ( userDTO.Password != null )
            {
                user.Password = _securityService.PasswordHashing(userDTO.Password);
            }
            _dbContext.User.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync( Guid userId )
    {
        var user = await _dbContext.User.FindAsync(userId);
        if ( user != null )
        {
            _dbContext.User.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<LoginSecurityDTO> LoginAsync( UserLoginDTO userLoginDTO )
    {
        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Email == userLoginDTO.Email);

        if ( user == null || _securityService.PasswordHashing(userLoginDTO.Password) != user.Password )
        {
            return new LoginSecurityDTO { Token = null, RequiresMfa = false, ChallengeId = null };
        }

        if ( !user.IsMfaEnabled )
        {
            string token = _securityService.CreateToken(user.Id); // Generate JWT token
            return new LoginSecurityDTO { Token = token, RequiresMfa = false, ChallengeId = null };
        }

        // Generate a temporary challenge ID for MFA verification
        string challengeId = Guid.NewGuid().ToString();
        _cache.Set($"mfa_challenge_{challengeId}", user.Id, TimeSpan.FromMinutes(5));

        return new LoginSecurityDTO { Token = null, RequiresMfa = true, ChallengeId = challengeId };
    }

    public async Task<string?> VerifyMfaAsync( MfaVerificationDTO verificationDTO )
    {
        if ( !_cache.TryGetValue($"mfa_challenge_{verificationDTO.ChallengeId}", out Guid userId) )
        {
            return null; // Challenge expired or invalid
        }

        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        if ( user == null || string.IsNullOrEmpty(user.MfaSecretKey) )
        {
            return null;
        }

        // Decode Base32 secret key
        byte[] secretKeyBytes = Google.Authenticator.Base32Encoding.ToBytes(user.MfaSecretKey);

        // Create TOTP instance with a 30-second time step (Google Authenticator standard)
        var totp = new Totp(secretKeyBytes, step: 30);
        // Generate expected OTP for the current time
        var expectedOtp = totp.ComputeTotp();
        Console.WriteLine($"Expected OTP: {expectedOtp}");
        bool isValid = totp.VerifyTotp(verificationDTO.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);


        if ( !isValid )
        {
            return null; // Invalid MFA code
        }

        _cache.Remove($"mfa_challenge_{verificationDTO.ChallengeId}"); // Remove challenge after successful verification

        return _securityService.CreateToken(user.Id); // Return JWT token
    }

    public async Task<byte[]?> MfaGenerate( Guid userId )
    {
        var authUser = await _dbContext.User.FindAsync(userId);

        if ( authUser == null || !string.IsNullOrEmpty(authUser.MfaSecretKey) )
        {
            return null;
        }

        // Generate a Base32-encoded secret key (20-byte key)
        var key = KeyGeneration.GenerateRandomKey(20);
        var secretKey = Google.Authenticator.Base32Encoding.ToString(key);
        Console.WriteLine($"Generated Secret Key: {secretKey}");

        authUser.MfaSecretKey = secretKey;
        authUser.IsMfaEnabled = true;

        _dbContext.User.Update(authUser);
        await _dbContext.SaveChangesAsync();

        // Correctly formatted OTP URL
        string issuer = Uri.EscapeDataString("MFA-Security"); // Application name
        string accountName = Uri.EscapeDataString(authUser.Email); // User email
        string otpauthUrl = $"otpauth://totp/{issuer}:{accountName}?secret={secretKey}&issuer={issuer}&digits=6";

        // Generate QR Code
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public async Task<bool> MfaDisable( Guid userId )
    {
        var authUser = await _dbContext.User.FindAsync(userId);

        if ( authUser == null )
        {
            return false;
        }
        authUser.MfaSecretKey = null;
        authUser.IsMfaEnabled = false;

        _dbContext.User.Update(authUser);
        await _dbContext.SaveChangesAsync();
        return true;
    }


}

