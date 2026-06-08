using System;
using System.Text.RegularExpressions;

namespace Blanquita.Domain.ValueObjects;

public class PhoneNumber
{
    private static readonly Regex MxPhoneRegex = new(@"^\d{10}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El número de teléfono no puede estar vacío.", nameof(value));

        // Clean spaces, hyphens, and other non-digits
        var cleaned = Regex.Replace(value, @"\s+|-|\(|\)", "");

        // If it starts with +52 or 52, strip it to validate the 10-digit local number
        if (cleaned.StartsWith("+52"))
        {
            cleaned = cleaned.Substring(3);
        }
        else if (cleaned.StartsWith("52") && cleaned.Length > 10)
        {
            cleaned = cleaned.Substring(2);
        }

        if (!MxPhoneRegex.IsMatch(cleaned))
        {
            throw new ArgumentException("El número de teléfono debe ser un número válido de México de 10 dígitos (ej. 3312345678).", nameof(value));
        }

        return new PhoneNumber(cleaned);
    }

    public override bool Equals(object? obj)
    {
        if (obj is PhoneNumber other)
        {
            return Value == other.Value;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
