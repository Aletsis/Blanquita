using System;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Representa un registro de auditoría para los cambios realizados en la configuración del sistema.
/// </summary>
public class SystemConfigurationAuditLog : BaseEntity
{
    public DateTime ChangedAt { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public string PropertyName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    // EF Core constructor
    private SystemConfigurationAuditLog() { }

    public SystemConfigurationAuditLog(DateTime changedAt, string changedBy, string propertyName, string? oldValue, string? newValue)
    {
        ChangedAt = changedAt;
        ChangedBy = changedBy;
        PropertyName = propertyName;

        // Mask sensitive values (passwords, API keys)
        if (IsSensitiveProperty(propertyName))
        {
            OldValue = string.IsNullOrWhiteSpace(oldValue) ? null : "[REDACTED]";
            NewValue = string.IsNullOrWhiteSpace(newValue) ? null : "[REDACTED]";
        }
        else
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return false;
        
        var name = propertyName.ToLowerInvariant();
        return name.Contains("password") || 
               name.Contains("key") || 
               name.Contains("secret") || 
               name.Contains("token");
    }
}
