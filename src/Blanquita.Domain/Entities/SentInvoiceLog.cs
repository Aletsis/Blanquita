using System;

namespace Blanquita.Domain.Entities;

public class SentInvoiceLog : BaseEntity
{
    public string ClientCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
