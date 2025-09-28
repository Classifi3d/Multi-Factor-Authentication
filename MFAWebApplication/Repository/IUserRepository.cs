using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using MFAWebApplication.Repository;

namespace AuthenticationWebApplication.Repository;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default );
}
