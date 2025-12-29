using Blanquita.Domain.Exceptions;
using Xunit;

namespace Blanquita.Domain.Tests.Exceptions;

public class DomainExceptionsTests
{
    [Fact]
    public void EntityNotFoundException_IdConstructor_ShouldMessageCorrectly()
    {
        var ex = new EntityNotFoundException("User", 1);
        Assert.Equal("User with ID 1 was not found", ex.Message);
    }

    [Fact]
    public void EntityNotFoundException_StringConstructor_ShouldMessageCorrectly()
    {
        var ex = new EntityNotFoundException("User", "admin");
        Assert.Equal("User with identifier 'admin' was not found", ex.Message);
    }

    [Fact]
    public void DuplicateEntityException_Constructor_ShouldMessageCorrectly()
    {
        var ex = new DuplicateEntityException("User", "test@test.com");
        Assert.Equal("User with identifier 'test@test.com' already exists", ex.Message);
    }


}
