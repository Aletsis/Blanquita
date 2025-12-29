namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

/// <summary>
/// Configuración para acceso a archivos FoxPro.
/// </summary>
public class FoxProSettings
{
    public const string SectionName = "FoxPro";

    public string Mgw10008Path { get; set; } = string.Empty;
    public string Mgw10005Path { get; set; } = string.Empty;
    public string Pos10041Path { get; set; } = string.Empty;
    public string Pos10042Path { get; set; } = string.Empty;
}
