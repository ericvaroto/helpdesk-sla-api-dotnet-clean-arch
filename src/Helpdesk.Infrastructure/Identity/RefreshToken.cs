using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Infrastructure.Identity
{
    public sealed class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}
