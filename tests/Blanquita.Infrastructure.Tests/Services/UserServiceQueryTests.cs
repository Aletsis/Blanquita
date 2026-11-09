using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class UserServiceQueryTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OrderByRole_TranslatesToSqlSubquery(bool ascending)
    {
        // ToQueryString genera el SQL sin conectarse: falla si EF no puede traducir el orden por rol
        using var context = new BlanquitaDbContext(new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseNpgsql("Host=localhost;Database=no_se_conecta")
            .Options);

        var sql = UserService.OrderByRole(context.Users, context, ascending).Skip(10).Take(10).ToQueryString();

        Assert.Contains("AspNetUserRoles", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("min(", sql, StringComparison.OrdinalIgnoreCase);
    }
}
