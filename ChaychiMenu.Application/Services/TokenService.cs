using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using ChaychiMenu.Application.Dto;
using ChaychiMenu.Application.ServiceContracts;

using ChaychiMenu.Shared.Settings;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChaychiMenu.Application.Services;

public class TokenService(
    IOptions<JwtSettings> jwtSettings, 
    IOptions<RefreshTokenSettings> refreshTokenSettings) : ITokenService
{
    public TokenDto GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var options = jwtSettings.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddMinutes(options.ValidFor);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenDto() { Token = tokenString, Expires = expires };
    }

    public TokenDto GenerateRefreshToken()
    {
        var options = refreshTokenSettings.Value;
        var randomNumber = new byte[32];
        
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var token = Convert.ToBase64String(randomNumber);
        return new TokenDto()
        {
            Token = token,
            Expires = DateTimeOffset.UtcNow.AddDays(options.ValidFor)
        };
    }
}