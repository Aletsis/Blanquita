using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetSalesReportByFilters;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Repositories;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetSalesReportByFiltersQueryHandlerTests
{
    private readonly Mock<IFoxProPedidoRepository> _mockPedidoRepository = new();
    private readonly Mock<ICashRegisterRepository> _mockCashRegisterRepository = new();
    private readonly Mock<IBranchRepository> _mockBranchRepository = new();
    private readonly Mock<IFoxProCashRegisterRepository> _mockFoxProCashRegisterRepository = new();
    private readonly GetSalesReportByFiltersQueryHandler _handler;

    public GetSalesReportByFiltersQueryHandlerTests()
    {
        _handler = new GetSalesReportByFiltersQueryHandler(
            _mockPedidoRepository.Object,
            _mockCashRegisterRepository.Object,
            _mockBranchRepository.Object,
            _mockFoxProCashRegisterRepository.Object);
    }

    [Fact]
    public async Task Handle_UnificaSeriesDeFoxProBaseDeDatosYSucursal()
    {
        var branch = Branch.Create("Himno", "B01", "COH", "FGIH", "DFCH", "Av. Himno", null);
        _mockBranchRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        _mockFoxProCashRegisterRepository
            .Setup(r => r.GetSeriesByBranchCodeAsync("B01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "CAJA1", "COH" });

        var register = CashRegister.Create("Caja 2", "CAJA2", 2, "192.168.1.10", 9100, 1);
        _mockCashRegisterRepository
            .Setup(r => r.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { register });

        IEnumerable<string>? capturedSeries = null;
        _mockPedidoRepository
            .Setup(r => r.GetSalesReportByFiltersAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<DateTime, DateTime, TimeSpan, TimeSpan, IEnumerable<string>, CancellationToken>(
                (_, _, _, _, s, _) => capturedSeries = s.ToList())
            .ReturnsAsync(new[] { new PedidoDto { Folio = "COH-1" } });

        var query = new GetSalesReportByFiltersQuery(
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 9),
            new TimeSpan(7, 0, 0),
            new TimeSpan(22, 0, 0),
            1);

        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.NotNull(capturedSeries);
        Assert.Contains("CAJA1", capturedSeries);
        Assert.Contains("CAJA2", capturedSeries);
        Assert.Contains("COH", capturedSeries);
        Assert.Contains("FGIH", capturedSeries);
        Assert.Contains("DFCH", capturedSeries);
    }

    [Fact]
    public async Task Handle_TodasLasSucursales_EnviaListaDeSeriesVaciaParaNoFiltrar()
    {
        IEnumerable<string>? capturedSeries = null;
        _mockPedidoRepository
            .Setup(r => r.GetSalesReportByFiltersAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<DateTime, DateTime, TimeSpan, TimeSpan, IEnumerable<string>, CancellationToken>(
                (_, _, _, _, s, _) => capturedSeries = s.ToList())
            .ReturnsAsync(new[] { new PedidoDto { Folio = "COH-1" } });

        var query = new GetSalesReportByFiltersQuery(
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 9),
            new TimeSpan(7, 0, 0),
            new TimeSpan(22, 0, 0),
            0);

        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.NotNull(capturedSeries);
        Assert.Empty(capturedSeries);
    }
}
