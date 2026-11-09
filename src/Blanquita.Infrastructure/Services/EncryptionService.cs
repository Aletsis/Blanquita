using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de encriptación usando AES-256.
/// </summary>
public class EncryptionService : IEncryptionService
{
    /// <summary>
    /// Prefijo que identifica los valores cifrados por esta versión del servicio.
    /// Permite distinguir con certeza un valor cifrado de uno guardado en texto plano.
    /// </summary>
    public const string EncryptedPrefix = "enc:v1:";

    private readonly byte[] _keyBytes;
    private readonly ILogger<EncryptionService>? _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService>? logger = null)
    {
        var secretKey = configuration["Encryption:Key"] ?? "BlanquitaAppEncryptionSecretKey2026";
        _keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsEncrypted(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(EncryptedPrefix, StringComparison.Ordinal);

    /// <inheritdoc/>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        using var ms = new MemoryStream();
        
        // Escribir el IV primero
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(plainText);
        }

        return EncryptedPrefix + Convert.ToBase64String(ms.ToArray());
    }

    /// <inheritdoc/>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        if (IsEncrypted(cipherText))
        {
            try
            {
                return DecryptPayload(cipherText.Substring(EncryptedPrefix.Length));
            }
            catch (Exception ex)
            {
                // El valor sí estaba cifrado: si no se puede descifrar, casi siempre es porque cambió Encryption:Key.
                // No se devuelve el texto cifrado para no usarlo como si fuera la credencial.
                _logger?.LogError(ex, "No se pudo descifrar un valor protegido. Verifique que Encryption:Key no haya cambiado; deberá capturar de nuevo la credencial.");
                return string.Empty;
            }
        }

        // Formato anterior sin prefijo (o texto plano): se conserva el comportamiento previo
        try
        {
            return DecryptPayload(cipherText);
        }
        catch
        {
            // Si falla la desencriptación (por ejemplo, si el texto ya estaba en plano y no es base64 válido),
            // retornamos el texto original para no romper configuraciones existentes.
            return cipherText;
        }
    }

    private string DecryptPayload(string base64Payload)
    {
        var fullCipher = Convert.FromBase64String(base64Payload);

        using var aes = Aes.Create();
        aes.Key = _keyBytes;

        var iv = new byte[aes.BlockSize / 8];

        if (fullCipher.Length < iv.Length)
        {
            throw new CryptographicException("El valor cifrado es demasiado corto.");
        }

        var cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);

        return sr.ReadToEnd();
    }
}
