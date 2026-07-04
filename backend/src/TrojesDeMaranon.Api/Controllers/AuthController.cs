using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrojesDeMaranon.Application.Auth;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Infrastructure.Authentication;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AppDbContext db,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    IValidator<LoginRequest> loginValidator,
    ICurrentUserService currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await db.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x!.RolePermissions).ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);

        if (user is null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Credenciales invalidas."));
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        var roles = user.UserRoles.Select(x => x.Role!.Name).ToArray();
        var permissions = user.UserRoles.SelectMany(x => x.Role!.RolePermissions.Select(rp => rp.Permission!.Code)).Distinct().ToArray();
        var accessToken = tokenService.CreateAccessToken(user, roles, permissions);
        var refreshToken = tokenService.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            CompanyId = user.CompanyId,
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays)
        });
        await db.SaveChangesAsync(cancellationToken);

        var me = new MeResponse(user.Id, user.CompanyId, user.FullName, user.Email, roles, permissions);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse(accessToken.AccessToken, refreshToken, accessToken.ExpiresAt, me));
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await db.RefreshTokens
            .Include(x => x.User).ThenInclude(x => x!.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x!.RolePermissions).ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive || refreshToken.User is null || !refreshToken.User.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Refresh token invalido."));
        }

        var user = refreshToken.User;
        var roles = user.UserRoles.Select(x => x.Role!.Name).ToArray();
        var permissions = user.UserRoles.SelectMany(x => x.Role!.RolePermissions.Select(rp => rp.Permission!.Code)).Distinct().ToArray();
        var newRefreshToken = tokenService.CreateRefreshToken();
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.ReplacedByTokenHash = tokenService.HashRefreshToken(newRefreshToken);
        db.RefreshTokens.Add(new RefreshToken
        {
            CompanyId = user.CompanyId,
            UserId = user.Id,
            TokenHash = refreshToken.ReplacedByTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays)
        });
        var accessToken = tokenService.CreateAccessToken(user, roles, permissions);
        await db.SaveChangesAsync(cancellationToken);
        var me = new MeResponse(user.Id, user.CompanyId, user.FullName, user.Email, roles, permissions);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse(accessToken.AccessToken, newRefreshToken, accessToken.ExpiresAt, me));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is not null)
        {
            var activeTokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null).ToListAsync(cancellationToken);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync(cancellationToken);
        }
        return ApiResponse<object>.Ok(new { });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<MeResponse>>> Me(CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x!.RolePermissions).ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<MeResponse>.Fail("Usuario no encontrado."));
        }

        var roles = user.UserRoles.Select(x => x.Role!.Name).ToArray();
        var permissions = user.UserRoles.SelectMany(x => x.Role!.RolePermissions.Select(rp => rp.Permission!.Code)).Distinct().ToArray();
        return ApiResponse<MeResponse>.Ok(new MeResponse(user.Id, user.CompanyId, user.FullName, user.Email, roles, permissions));
    }
}
