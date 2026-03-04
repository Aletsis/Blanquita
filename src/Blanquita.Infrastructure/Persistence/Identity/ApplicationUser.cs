using Microsoft.AspNetCore.Identity;

namespace Blanquita.Infrastructure.Persistence.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? FullName { get; set; }
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;

    public ApplicationUser()
    {
    }
}
