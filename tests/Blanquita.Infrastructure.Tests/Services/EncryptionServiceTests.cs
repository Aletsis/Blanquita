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

    [Fact]
    public void Encrypt_ShouldAddVersionPrefix_AndIsEncryptedShouldDetectIt()
    {
        var service = new EncryptionService(_configMock.Object);

        var encrypted = service.Encrypt("api-key-123");

        Assert.StartsWith(EncryptionService.EncryptedPrefix, encrypted);
        Assert.True(service.IsEncrypted(encrypted));
        Assert.False(service.IsEncrypted("api-key-123"));
        Assert.False(service.IsEncrypted(string.Empty));
        Assert.False(service.IsEncrypted(null));
    }

    [Fact]
    public void Decrypt_ShouldSupportLegacyValuesWithoutPrefix()
    {
        // Valores cifrados con la versión anterior (sin prefijo) deben seguir descifrándose
        var service = new EncryptionService(_configMock.Object);
        var legacy = service.Encrypt("smtp-password").Substring(EncryptionService.EncryptedPrefix.Length);

        Assert.Equal("smtp-password", service.Decrypt(legacy));
    }

    [Fact]
    public void Decrypt_ShouldReturnEmpty_WhenPrefixedValueCannotBeDecrypted()
    {
        // Cifrado con una llave y descifrado con otra (p. ej. cambió Encryption:Key)
        var configA = new Mock<IConfiguration>();
        configA.Setup(c => c["Encryption:Key"]).Returns("llave-A");
        var configB = new Mock<IConfiguration>();
        configB.Setup(c => c["Encryption:Key"]).Returns("llave-B");

        var encrypted = new EncryptionService(configA.Object).Encrypt("secreto");
        var result = new EncryptionService(configB.Object).Decrypt(encrypted);

        // No debe devolver el texto cifrado como si fuera la credencial
        Assert.NotEqual(encrypted, result);
        Assert.NotEqual("secreto", result);
    }
}
