using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace API.Interfaces;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var tokenKey = config["TokenKey"] ?? throw new Exception("Can't access tokenKey from app settings configuration");
        if(tokenKey.Length < 64) throw new Exception("Your tokenKey needs to be longer");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)); // pretvaramo ga u simetricni kljuc , mora u byte[]

        // jwt ce koristiti claimove o useru

        // claims
        var claims = new List<Claim>{
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
        };

        // credentials are using to sign the token
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        
        // TOKEN - descriptor
        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        // Token handler - for creating token
        var tokenHandler =  new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
