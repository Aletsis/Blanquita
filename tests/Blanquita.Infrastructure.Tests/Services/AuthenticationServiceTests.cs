using Blanquita.Infrastructure.Persistence.Identity;
using Blanquita.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        // Mock UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        // Mock SignInManager
        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            claimsFactoryMock.Object,
            null, null, null, null);

        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _service = new AuthenticationService(_signInManagerMock.Object, _userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTrue_WhenCredentialsValid()
    {
        _signInManagerMock.Setup(x => x.PasswordSignInAsync("admin", "password", true, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await _service.AuthenticateAsync("admin", "password");

        Assert.True(result);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFalse_WhenCredentialsInvalid()
    {
        _signInManagerMock.Setup(x => x.PasswordSignInAsync("admin", "wrong", true, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _service.AuthenticateAsync("admin", "wrong");

        Assert.False(result);
    }

    [Fact]
    public async Task IsAdministratorAsync_ShouldReturnTrue_WhenUserExists()
    {
        var user = new ApplicationUser { UserName = "admin" };
        _userManagerMock.Setup(x => x.FindByNameAsync("admin"))
            .ReturnsAsync(user);

        var result = await _service.IsAdministratorAsync("admin");

        Assert.True(result);
    }

    [Fact]
    public async Task IsAdministratorAsync_ShouldReturnFalse_WhenUserNotExists()
    {
        _userManagerMock.Setup(x => x.FindByNameAsync("nonexistent"))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.IsAdministratorAsync("nonexistent");

        Assert.False(result);
    }

    [Fact]
    public async Task LogoutAsync_ShouldCallSignOut()
    {
        await _service.LogoutAsync();

        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }
}
