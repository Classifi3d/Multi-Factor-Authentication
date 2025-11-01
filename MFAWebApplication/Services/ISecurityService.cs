namespace MFAWebApplication.Services;

public interface ISecurityService
{
    public string CreateToken( Guid userId );
    public string PasswordHashing( string inputString );
}
