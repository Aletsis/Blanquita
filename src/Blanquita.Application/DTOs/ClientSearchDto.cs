using System.Collections.Generic;

namespace Blanquita.Application.DTOs;

public class ClientSearchDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CfdiUse { get; set; } = string.Empty;
    public string FiscalRegime { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public List<ClientAddressDto> Addresses { get; set; } = new();
}
