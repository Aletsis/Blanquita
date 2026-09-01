using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetOpenPedidosReport;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetOpenPedidosReportQueryHandlerTests
{
    private readonly Mock<IFoxProPedidoRepository> _mockRepository;
    private readonly GetOpenPedidosReportQueryHandler _handler;

    public GetOpenPedidosReportQueryHandlerTests()
    {
        _mockRepository = new Mock<IFoxProPedidoRepository>();
        _handler = new GetOpenPedidosReportQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_OpenPedidosExist_ReturnsItems()
    {
        // Arrange
        var startDate = new DateTime(2026, 8, 1);
        var endDate = new DateTime(2026, 8, 31);
        var series = new List<string> { "PA", "PB" };
        var expectedItems = new List<OpenPedidoReportItemDto>
        {
            new OpenPedidoReportItemDto
            {
                IdDocumento = "PED-001",
                Fecha = new DateTime(2026, 8, 20),
                Serie = "PA",
                Folio = "5001",
                Comanda = "1001",
                Sucursal = "Matriz",
                Cliente = "Cliente Frecuente",
                Ruta = "R1",
                Repartidor = "Juan Pérez",
                DiasAbierto = 5,
                Neto = 300m,
                Impuesto = 48m,
                Total = 348m,
                PartidasCount = 3
            }
        };

        _mockRepository
            .Setup(r => r.GetOpenPedidosReportAsync(startDate, endDate, series, "1001", "R1", "Matriz", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        var query = new GetOpenPedidosReportQuery(startDate, endDate, series, "1001", "R1", "Matriz");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal("PA", list[0].Serie);
        Assert.Equal("5001", list[0].Folio);
        Assert.Equal("1001", list[0].Comanda);
        Assert.Equal(5, list[0].DiasAbierto);
        Assert.Equal(348m, list[0].Total);

        _mockRepository.Verify(r => r.GetOpenPedidosReportAsync(startDate, endDate, series, "1001", "R1", "Matriz", It.IsAny<CancellationToken>()), Times.Once);
    }
}
