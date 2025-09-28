using AuthenticationWebApplication.Context;
using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AuthenticationWebApplication.Repository;
using MFAWebApplication.Repository;
using Microsoft.EntityFrameworkCore;

public class UserRepository : Repository<User, Guid>, IUserRepository
{
    public UserRepository( ApplicationDbContext context ) : base(context) { }

    public async Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default )
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
