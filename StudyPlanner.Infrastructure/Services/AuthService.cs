using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudyPlanner.Core.Constants;
using StudyPlanner.Core.DTOs.Auth;
using StudyPlanner.Core.Entities;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Interfaces.Services;
using StudyPlanner.Core.Settings;

namespace StudyPlanner.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            throw new BadRequestException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DateCreated = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, Roles.Student);

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public Task LogoutAsync(string userId, CancellationToken cancellationToken = default) =>
        _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("Invalid access token.");
        }

        var storedToken = await _tokenService.GetRefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (storedToken is null || storedToken.UserId != userId)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new UnauthorizedException("User not found.");
        }

        await _tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, cancellationToken);

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshExpires, cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = roles,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiration()
        };
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Invalid access token.");
        }

        return principal;
    }
}
