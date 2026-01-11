using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Helpdesk.Infrastructure.Identity
{
    public sealed class AppUser : IdentityUser<Guid>
    {
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
