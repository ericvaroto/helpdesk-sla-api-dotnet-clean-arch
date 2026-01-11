using Helpdesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controller
{
    [ApiController]
    [Route("dev/seed")]
    public sealed class DevSeedController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public DevSeedController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("admin")]
        public async Task<IActionResult> SeedAdmin()
        {
            if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? true)
                return NotFound();

            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var roles = new[] { "TenantAdmin", "Manager", "Agent", "Requester" };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(r));
            }

            var email = "admin@local.test";
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    TenantId = tenantId,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var created = await _userManager.CreateAsync(user, "Admin@12345");
                if (!created.Succeeded) return BadRequest(created.Errors);
            }

            if (!await _userManager.IsInRoleAsync(user, "TenantAdmin"))
                await _userManager.AddToRoleAsync(user, "TenantAdmin");

            return Ok(new
            {
                Email = email,
                Password = "Admin@12345",
                TenantId = tenantId
            });
        }
    }
}