namespace Blanquita.Application.Interfaces;

/// <summary>
/// Interfaz para servicios de encriptación y desencriptación.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encripta un texto en plano a una cadena cifrada en Base64.
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Desencripta un texto cifrado en Base64 a su valor original en plano.
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// Indica si el valor fue cifrado con el formato actual (con prefijo de versión).
    /// Los valores sin prefijo son texto plano o del formato anterior.
    /// </summary>
    bool IsEncrypted(string? value);
}
