using Microsoft.AspNetCore.Identity;

namespace Blanquita.Infrastructure.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
