using Helpdesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Persistence
{
    public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Importante: Index para consultas multi-tenant
            builder.Entity<AppUser>()
                .HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique(false);

            builder.Entity<RefreshToken>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.TokenHash).IsUnique();
            });
        }
    }
}