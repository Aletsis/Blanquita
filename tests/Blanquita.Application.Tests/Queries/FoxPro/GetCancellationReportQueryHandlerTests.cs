using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetCancellationReport;
using Blanquita.Domain.Repositories;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetCancellationReportQueryHandlerTests
{
    private readonly Mock<IFoxProDocumentRepository> _mockRepository;
    private readonly GetCancellationReportQueryHandler _handler;

    public GetCancellationReportQueryHandlerTests()
    {
        _mockRepository = new Mock<IFoxProDocumentRepository>();
        _handler = new GetCancellationReportQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_CancellationsExist_ReturnsItems()
    {
        // Arrange
        var startDate = new DateTime(2026, 8, 1);
        var endDate = new DateTime(2026, 8, 31);
        var expectedItems = new List<CancellationReportItemDto>
        {
            new CancellationReportItemDto
            {
                IdDocumento = "DOC-001",
                Fecha = new DateTime(2026, 8, 10),
                Serie = "A",
                Folio = "1001",
                TipoCancelacion = "Completa",
                TipoDocumento = "Venta POS",
                Neto = 100m,
                Impuesto = 16m,
                Total = 116m
            },
            new CancellationReportItemDto
            {
                IdDocumento = "DOC-002",
                Fecha = new DateTime(2026, 8, 12),
                Serie = "A",
                Folio = "1002",
                TipoCancelacion = "Parcial",
                TipoDocumento = "Venta POS",
                Neto = 50m,
                Impuesto = 8m,
                Total = 58m
            }
        };

        _mockRepository
            .Setup(r => r.GetCancellationsReportAsync(startDate, endDate, "A", "Todas", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        var query = new GetCancellationReportQuery(startDate, endDate, "A", "Todas");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("Completa", list[0].TipoCancelacion);
        Assert.Equal("Parcial", list[1].TipoCancelacion);
        Assert.Equal(116m, list[0].Total);

        _mockRepository.Verify(r => r.GetCancellationsReportAsync(startDate, endDate, "A", "Todas", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FilterByParcial_CallsRepositoryWithParcialType()
    {
        // Arrange
        var startDate = new DateTime(2026, 8, 1);
        var endDate = new DateTime(2026, 8, 31);
        var expectedItems = new List<CancellationReportItemDto>
        {
            new CancellationReportItemDto
            {
                IdDocumento = "DOC-002",
                Fecha = new DateTime(2026, 8, 12),
                Serie = "A",
                Folio = "1002",
                TipoCancelacion = "Parcial",
                TipoDocumento = "Venta POS",
                Neto = 50m,
                Impuesto = 8m,
                Total = 58m,
                PartidasCanceladasCount = 1,
                Detalles = new List<CancellationDetailDto>
                {
                    new CancellationDetailDto
                    {
                        ProductId = "PROD-1",
                        ProductName = "Producto A",
                        Units = 1,
                        Price = 58m,
                        Total = 58m
                    }
                }
            }
        };

        _mockRepository
            .Setup(r => r.GetCancellationsReportAsync(startDate, endDate, "A", "Parcial", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        var query = new GetCancellationReportQuery(startDate, endDate, "A", "Parcial");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal("Parcial", list[0].TipoCancelacion);
        Assert.Single(list[0].Detalles);
        Assert.Equal(58m, list[0].Total);

        _mockRepository.Verify(r => r.GetCancellationsReportAsync(startDate, endDate, "A", "Parcial", It.IsAny<CancellationToken>(), null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithBranchId_ResolvesAllBranchSeries()
    {
        // Arrange
        var mockBranchRepo = new Mock<Blanquita.Domain.Repositories.IBranchRepository>();
        var mockCashRegisterRepo = new Mock<ICashRegisterRepository>();
        var mockFoxProCashRegisterRepo = new Mock<IFoxProCashRegisterRepository>();

        var branch = Blanquita.Domain.Entities.Branch.Create(
            "Himno", "H01", "FGIH", "DFCH", "DEVH", "Av Himno", null);

        mockBranchRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        mockFoxProCashRegisterRepo
            .Setup(r => r.GetSeriesByBranchCodeAsync("H01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "COH", "CAJA1" });

        var register = Blanquita.Domain.Entities.CashRegister.Create("Caja 2", "CAJA2", 2, "192.168.1.10", 9100, 1);
        mockCashRegisterRepo
            .Setup(r => r.GetByBranchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { register });

        IEnumerable<string>? capturedSeries = null;
        _mockRepository
            .Setup(r => r.GetCancellationsReportAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                null,
                "Todas",
                It.IsAny<CancellationToken>(),
                It.IsAny<IEnumerable<string>>()))
            .Callback<DateTime?, DateTime?, string?, string?, CancellationToken, IEnumerable<string>?>((_, _, _, _, _, s) => capturedSeries = s)
            .ReturnsAsync(new List<CancellationReportItemDto>());

        var handler = new GetCancellationReportQueryHandler(
            _mockRepository.Object,
            mockBranchRepo.Object,
            mockCashRegisterRepo.Object,
            mockFoxProCashRegisterRepo.Object);

        var query = new GetCancellationReportQuery(
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 10),
            Tipo: "Todas",
            BranchId: 1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedSeries);
        var seriesList = capturedSeries.ToList();
        Assert.Contains("COH", seriesList);
        Assert.Contains("CAJA1", seriesList);
        Assert.Contains("CAJA2", seriesList);
        Assert.Contains("FGIH", seriesList);
        Assert.Contains("DFCH", seriesList);
        Assert.Contains("DEVH", seriesList);
    }
}
