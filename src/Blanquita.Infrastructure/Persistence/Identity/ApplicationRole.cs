using Microsoft.AspNetCore.Identity;

namespace Blanquita.Infrastructure.Persistence.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base() { }
    
    public ApplicationRole(string roleName) : base(roleName) { }
}
