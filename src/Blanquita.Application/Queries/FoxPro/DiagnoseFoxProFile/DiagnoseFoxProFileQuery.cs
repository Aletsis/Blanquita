using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.DiagnoseFoxProFile;

/// <summary>
/// Query para diagnosticar un archivo FoxPro/DBF.
/// </summary>
public record DiagnoseFoxProFileQuery(
    string Path, 
    List<string>? ExpectedColumns = null) : IRequest<DiagnosticoResultado>;
