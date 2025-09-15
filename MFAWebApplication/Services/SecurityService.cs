using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MFAWebApplication.Services;

public class SecurityService : ISecurityService
{

    private readonly IConfiguration _configuration;

    public SecurityService( IConfiguration configuration )
    {
        _configuration = configuration;
    }

    public string CreateToken( Guid userId ){
        List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.NameIdentifier,userId.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public string PasswordHashing( string inputString )
    {
        var inputBytes = Encoding.UTF8.GetBytes(inputString);
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }

}
