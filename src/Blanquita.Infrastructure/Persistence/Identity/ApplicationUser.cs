using Microsoft.AspNetCore.Identity;

namespace Blanquita.Infrastructure.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public override string? PhoneNumber { get; set; }
    public int? BranchId { get; set; }
}
