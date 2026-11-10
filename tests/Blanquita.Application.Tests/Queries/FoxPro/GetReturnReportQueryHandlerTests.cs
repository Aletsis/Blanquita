using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.FoxPro.GetReturnReport;
using Blanquita.Domain.Repositories;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries.FoxPro;

public class GetReturnReportQueryHandlerTests
{
    private readonly Mock<IFoxProDocumentRepository> _mockRepository;
    private readonly GetReturnReportQueryHandler _handler;

    public GetReturnReportQueryHandlerTests()
    {
        _mockRepository = new Mock<IFoxProDocumentRepository>();
        _handler = new GetReturnReportQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsExist_ReturnsItemsWithReferencia()
    {
        // Arrange
        var year = 2026;
        var month = 8;
        var serie = "DFCH";
        var expectedItems = new List<ReturnReportItemDto>
        {
            new ReturnReportItemDto
            {
                Fecha = new DateTime(2026, 8, 15),
                Serie = "DFCH",
                Folio = "101",
                Referencia = "DEV-501, DEV-502",
                Neto = 1000m,
                Impuesto = 160m,
                Total = 1160m
            },
            new ReturnReportItemDto
            {
                Fecha = new DateTime(2026, 8, 20),
                Serie = "DFCH",
                Folio = "102",
                Referencia = "DEV-503",
                Neto = 500m,
                Impuesto = 80m,
                Total = 580m
            }
        };

        _mockRepository
            .Setup(r => r.GetReturnsReportAsync(year, month, serie, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<IEnumerable<string>?>()))
            .ReturnsAsync(expectedItems);

        var query = new GetReturnReportQuery(year, month, serie);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("DFCH", resultList[0].Serie);
        Assert.Equal("101", resultList[0].Folio);
        Assert.Equal("DEV-501, DEV-502", resultList[0].Referencia);
        Assert.Equal(1160m, resultList[0].Total);

        Assert.Equal("DEV-503", resultList[1].Referencia);

        _mockRepository.Verify(r => r.GetReturnsReportAsync(year, month, serie, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<IEnumerable<string>?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoReturnsFound_ReturnsEmptyList()
    {
        // Arrange
        var year = 2026;
        var month = 8;
        var serie = "DFCH";

        _mockRepository
            .Setup(r => r.GetReturnsReportAsync(year, month, serie, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<IEnumerable<string>?>()))
            .ReturnsAsync(new List<ReturnReportItemDto>());

        var query = new GetReturnReportQuery(year, month, serie);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetReturnsReportAsync(year, month, serie, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<IEnumerable<string>?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithBranchId_ResolvesAllBranchSeries()
    {
        // Arrange
        var mockBranchRepo = new Mock<IBranchRepository>();
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
            .Setup(r => r.GetReturnsReportAsync(
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                null,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                "Todas",
                It.IsAny<CancellationToken>(),
                It.IsAny<IEnumerable<string>?>()))
            .Callback<int?, int?, string?, DateTime?, DateTime?, string?, CancellationToken, IEnumerable<string>?>((_, _, _, _, _, _, _, s) => capturedSeries = s)
            .ReturnsAsync(new List<ReturnReportItemDto>());

        var handler = new GetReturnReportQueryHandler(
            _mockRepository.Object,
            mockBranchRepo.Object,
            mockCashRegisterRepo.Object,
            mockFoxProCashRegisterRepo.Object);

        var query = new GetReturnReportQuery(
            StartDate: new DateTime(2026, 11, 1),
            EndDate: new DateTime(2026, 11, 10),
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
