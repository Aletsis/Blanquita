using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Blanquita.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de encriptación usando AES-256.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _keyBytes;

    public EncryptionService(IConfiguration configuration)
    {
        var secretKey = configuration["Encryption:Key"] ?? "BlanquitaAppEncryptionSecretKey2026";
        _keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
    }

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

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <inheritdoc/>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _keyBytes;

            var iv = new byte[aes.BlockSize / 8];
            
            if (fullCipher.Length < iv.Length)
            {
                return cipherText;
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
        catch
        {
            // Si falla la desencriptación (por ejemplo, si el texto ya estaba en plano y no es base64 válido),
            // retornamos el texto original para no romper configuraciones existentes.
            return cipherText;
        }
    }
}
