using Helpdesk.Application.Auth;
using Helpdesk.Infrastructure.Identity;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api.Controller
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private const int RefreshDays = 7;

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwt;

        public AuthController(UserManager<AppUser> userManager, AppDbContext db, JwtTokenService jwt)
        {
            _userManager = userManager;
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null || !user.IsActive) return Unauthorized();

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid) return Unauthorized();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Requester";

            var accessToken = _jwt.GenerateAccessToken(user.Id, role, user.TenantId);

            var (rawRefresh, refreshHash) = JwtTokenService.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshDays)
            });

            await _db.SaveChangesAsync();

            return Ok(new AuthResponse(accessToken, rawRefresh));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest();

            var hash = JwtTokenService.HashToken(request.RefreshToken);

            var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (token is null || !token.IsActive) return Unauthorized();

            var user = await _userManager.FindByIdAsync(token.UserId.ToString());
            if (user is null || !user.IsActive) return Unauthorized();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Requester";

            // Rotate refresh token: revoke old, issue new
            token.RevokedAt = DateTime.UtcNow;

            var (newRaw, newHash) = JwtTokenService.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshDays)
            });

            await _db.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(user.Id, role, user.TenantId);
            return Ok(new AuthResponse(accessToken, newRaw));
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest();

            var hash = JwtTokenService.HashToken(request.RefreshToken);

            var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (token is null) return NoContent();

            if (token.RevokedAt == null)
            {
                token.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.TenantId,
                Roles = roles
            });
        }
    }
}