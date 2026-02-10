using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetReturnBySeriesAndFolio;

/// <summary>
/// Query to find a return by series and folio.
/// </summary>
public record GetReturnBySeriesAndFolioQuery(string Series, string Folio) : IRequest<ReturnDto?>;
