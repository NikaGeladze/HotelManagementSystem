using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HMS.Application.Interfaces.Services;
using HMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HMS.Application.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secret = configuration.GetValue<string>("JwtSettings:secret");
        _issuer = configuration.GetValue<string>("JwtSettings:issuer");
        _audience = configuration.GetValue<string>("JwtSettings:audience");
    }
    public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(_secret);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
            new Claim(JwtRegisteredClaimNames.Name, applicationUser.UserName),
            new Claim("PersonalNumber", applicationUser.PersonalNumber),
            new Claim("fullname", $"{applicationUser.FirstName} {applicationUser.LastName}")
        };
        
        if (!string.IsNullOrEmpty(applicationUser.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email));
        
        if (!string.IsNullOrEmpty(applicationUser.PhoneNumber))
            claims.Add(new Claim("phone_number", applicationUser.PhoneNumber));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}