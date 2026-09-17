using System.Security.Claims;

using ChaychiMenu.Application.Commands.Owner;
using ChaychiMenu.Application.Dto;
using ChaychiMenu.Application.ServiceContracts;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

namespace ChaychiMenu.Application.Handlers.Owner;

public class ValidateOtpCommandHandler(
    IDistributedCache cache,
    UserManager<Domain.Entities.AppUser> userManager,
    ITokenService tokenService) : IRequestHandler<ValidateOtpCommand, ErrorOr<AuthDto>>
{
    public async Task<ErrorOr<AuthDto>> Handle(ValidateOtpCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"tg_otp:{request.Otp}";
        var userId = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (string.IsNullOrEmpty(userId))
            return Error.Validation("Otp.InvalidOrExpired", "Otp is wrong or expired.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("User.NotFound", "User not found.");

        user.TelegramId = request.TelegramId;
        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return Error.Failure("User.UpdateFailed", "Could not update user.");

        await cache.RemoveAsync(cacheKey, cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>([
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("jti", Guid.NewGuid().ToString()),
        ]);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessTokenData = tokenService.GenerateAccessToken(claims);
        var refreshTokenData = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshTokenData.Token;
        user.RefreshTokenExpires = refreshTokenData.Expires;
        
        return new AuthDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            AccessToken = accessTokenData.Token,
            AccessTokenExpiration = accessTokenData.Expires,
            RefreshToken = refreshTokenData.Token,
            RefreshTokenExpiration = refreshTokenData.Expires,
        };
    }
}