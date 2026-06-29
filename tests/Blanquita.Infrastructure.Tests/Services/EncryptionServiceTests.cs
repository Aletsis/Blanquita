using Blanquita.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class EncryptionServiceTests
{
    private readonly Mock<IConfiguration> _configMock;

    public EncryptionServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
    }

    [Fact]
    public void Encrypt_And_Decrypt_ShouldWorkCorrectly_WithDefaultKey()
    {
        // Arrange
        _configMock.Setup(c => c["Encryption:Key"]).Returns((string?)null);
        var service = new EncryptionService(_configMock.Object);
        var plainText = "MySuperSecretSmtpPassword123!";

        // Act
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_And_Decrypt_ShouldWorkCorrectly_WithCustomKey()
    {
        // Arrange
        _configMock.Setup(c => c["Encryption:Key"]).Returns("CustomEncryptionKeyForTesting12345");
        var service = new EncryptionService(_configMock.Object);
        var plainText = "AnotherPassword_456";

        // Act
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalText_WhenDecryptionFailsOrPlainText()
    {
        // Arrange
        _configMock.Setup(c => c["Encryption:Key"]).Returns("KeyToFail");
        var service = new EncryptionService(_configMock.Object);
        var plainText = "AlreadyPlainTextPassword";

        // Act
        var result = service.Decrypt(plainText);

        // Assert
        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var service = new EncryptionService(_configMock.Object);

        // Act & Assert
        Assert.Equal(string.Empty, service.Encrypt(string.Empty));
        Assert.Equal(string.Empty, service.Decrypt(string.Empty));
    }
}
