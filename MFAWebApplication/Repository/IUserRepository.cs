using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using MFAWebApplication.DTOs;

namespace AuthenticationWebApplication.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDTO>?> GetUsersAsync();
        Task<User> GetUserByIdAsync(Guid userId);
        Task<bool> AddUserAsync(UserDTO userDTO);
        Task<bool> UpdateUserAsync(UserDTO userDTO);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<LoginSecurityDTO> LoginAsync(UserLoginDTO userLoginDTO);
        Task<string?> VerifyMfaAsync(MfaVerificationDTO verificationDTO);
        Task<byte[]> MfaGenerate(Guid userId);
        Task<bool> MfaDisable(Guid userId);

    }
}
