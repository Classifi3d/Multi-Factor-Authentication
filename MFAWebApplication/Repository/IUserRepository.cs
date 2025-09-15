using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using MFAWebApplication.DTOs;

namespace AuthenticationWebApplication.Repository
{
    public interface IUserRepository
    {
        //Task<IEnumerable<UserDTO>?> GetUsersAsync();
        Task<User?> GetByIdAsync( Guid userId , CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync( string email, CancellationToken cancellationToken = default );
        Task AddAsync( User user, CancellationToken cancellationToken = default );
        void Update( User user );
        void Delete( User user );

        //Task<bool> AddUserAsync(UserDTO userDTO);
        //Task<bool> UpdateUserAsync(UserDTO userDTO);
        //Task<bool> DeleteUserAsync(Guid userId);
        //Task<LoginSecurityDTO> LoginAsync(UserLoginDTO userLoginDTO);
        //Task<string?> VerifyMfaAsync(MfaVerificationDTO verificationDTO);
        //Task<byte[]> MfaGenerate(Guid userId);
        //Task<bool> MfaDisable(Guid userId);



    }
}
