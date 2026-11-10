using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Queries.Cajas;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Queries;

public class GetMonitorCajasQueryHandlerTests
{
    private readonly Mock<IFoxProShiftRepository> _mockFoxProShiftRepo;
    private readonly Mock<ICashRegisterRepository> _mockCashRegisterRepo;
    private readonly Mock<IBranchRepository> _mockBranchRepo;
    private readonly Mock<ICashierRepository> _mockCashierRepo;
    private readonly Mock<ICashCollectionRepository> _mockCashCollectionRepo;
    private readonly Mock<IConciliacionCorteRepository> _mockConciliacionRepo;
    private readonly Mock<IConfiguracionService> _mockConfigService;
    private readonly Mock<ILogger<GetMonitorCajasQueryHandler>> _mockLogger;
    private readonly GetMonitorCajasQueryHandler _handler;

    private static T SetId<T>(T entity, int id) where T : BaseEntity
    {
        typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(entity, id);
        return entity;
    }

    public GetMonitorCajasQueryHandlerTests()
    {
        _mockFoxProShiftRepo = new Mock<IFoxProShiftRepository>();
        _mockCashRegisterRepo = new Mock<ICashRegisterRepository>();
        _mockBranchRepo = new Mock<IBranchRepository>();
        _mockCashierRepo = new Mock<ICashierRepository>();
        _mockCashCollectionRepo = new Mock<ICashCollectionRepository>();
        _mockConciliacionRepo = new Mock<IConciliacionCorteRepository>();
        _mockConfigService = new Mock<IConfiguracionService>();
        _mockLogger = new Mock<ILogger<GetMonitorCajasQueryHandler>>();

        _mockConfigService.Setup(c => c.ObtenerConfiguracionAsync())
            .ReturnsAsync(new ConfiguracionDto());

        _handler = new GetMonitorCajasQueryHandler(
            _mockFoxProShiftRepo.Object,
            _mockCashRegisterRepo.Object,
            _mockBranchRepo.Object,
            _mockCashierRepo.Object,
            _mockCashCollectionRepo.Object,
            _mockConciliacionRepo.Object,
            _mockConfigService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenNoShiftsExist_ReturnsEmptyList()
    {
        // Arrange
        var date = DateTime.Today;
        _mockFoxProShiftRepo
            .Setup(r => r.GetTodayShiftsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftConciliationDataDto>());

        var query = new GetMonitorCajasQuery(date, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenShiftsExist_ReturnsMappedMonitorDtosWithCalculations()
    {
        // Arrange
        var date = DateTime.Today;
        var openingTime = date.AddHours(8);

        var shifts = new List<ShiftConciliationDataDto>
        {
            new()
            {
                InternalId = 101,
                IdContpaqi = 1,
                CashierId = 50,
                OpeningTime = openingTime,
                ClosingTime = null,
                Status = 0, // Abierta
                CashCollected = 15000m,
                CardCollected = 5000m,
                ReturnsTotal = -200m
            }
        };

        var branch = SetId(Branch.Create("Sucursal Centro", "SC", "A", "B", "DEV", "Calle 1", null), 1);
        var cashRegister = SetId(CashRegister.Create("Caja 1", "A", 1, "192.168.1.100", 9100, branch.Id, false, TipoTerminal.PisoVentas), 10);
        var cashier = Cashier.Create(1001, "María López", branch.Id, 50);

        _mockFoxProShiftRepo
            .Setup(r => r.GetTodayShiftsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _mockBranchRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { branch });

        _mockCashRegisterRepo
            .Setup(r => r.GetByBranchAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashRegister> { cashRegister });

        _mockCashierRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cashier> { cashier });

        _mockConciliacionRepo
            .Setup(r => r.GetAlreadyConciliatedShiftIdsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<int>());

        // 1 recolección de $4,000 (8 billetes de $500)
        var reco = CashCollection.Create(0, 8, 0, 0, 0, 0, "Caja 1", "María López", "Supervisor", 1, false);
        _mockCashCollectionRepo
            .Setup(r => r.GetCollectionsByRegisterAndTimeAsync("Caja 1", openingTime, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashCollection> { reco });

        var query = new GetMonitorCajasQuery(date, branch.Id);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("Caja 1", dto.CashRegisterName);
        Assert.Equal("María López", dto.CashierName);
        Assert.Equal(15000m, dto.CashCollected);
        Assert.Equal(5000m, dto.CardCollected);
        Assert.Equal(20000m, dto.TotalSold);
        Assert.Equal(-200m, dto.ReturnsTotal);
        Assert.Equal(4000m, dto.TotalRecolectado);
        Assert.Single(dto.Recolecciones);
        Assert.Equal(11000m, dto.EfectivoEstimadoEnCaja); // 15000 - 4000
        Assert.False(dto.IsClosed);
        Assert.False(dto.IsConciliated);
    }

    [Fact]
    public async Task Handle_WhenShiftIsClosedAndConciliated_MarksStatusAndLoadsSalidas()
    {
        // Arrange
        var date = DateTime.Today;
        var openingTime = date.AddHours(8);
        var closingTime = date.AddHours(16);

        var shifts = new List<ShiftConciliationDataDto>
        {
            new()
            {
                InternalId = 202,
                IdContpaqi = 2,
                CashierId = 60,
                OpeningTime = openingTime,
                ClosingTime = closingTime,
                Status = 1, // Cerrada
                CashCollected = 10000m,
                CardCollected = 2000m,
                ReturnsTotal = 0m
            }
        };

        var branch = SetId(Branch.Create("Sucursal Norte", "SN", "A", "B", "DEV", "Calle 2", null), 2);
        var cashRegister = SetId(CashRegister.Create("Caja 2", "B", 2, "192.168.1.102", 9100, branch.Id, false, TipoTerminal.PisoVentas), 20);

        _mockFoxProShiftRepo
            .Setup(r => r.GetTodayShiftsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _mockBranchRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { branch });

        _mockCashRegisterRepo
            .Setup(r => r.GetByBranchAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashRegister> { cashRegister });

        _mockCashierRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Cashier>());

        _mockConciliacionRepo
            .Setup(r => r.GetAlreadyConciliatedShiftIdsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 202 });

        var conciliacion = new ConciliacionCorte(
            202, "Sucursal Norte", "Caja 2", "Cajera #60", 0, 9500, 500, 10000, 0, 0, 2000, 0, 12000, 12000, 0, date, null, "Supervisor");
        SetId(conciliacion, 99);

        _mockConciliacionRepo
            .Setup(r => r.GetByAperturaIdAsync(202, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conciliacion);

        _mockCashCollectionRepo
            .Setup(r => r.GetCollectionsByRegisterAndTimeAsync("Caja 2", openingTime, closingTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CashCollection>());

        var query = new GetMonitorCajasQuery(date, branch.Id);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.True(dto.IsClosed);
        Assert.True(dto.IsConciliated);
        Assert.Equal(99, dto.ConciliacionId);
        Assert.Equal(500m, dto.SalidasEfectivo);
        Assert.Equal(9500m, dto.EfectivoEstimadoEnCaja); // 10000 - 0 (recos) - 500 (salidas)
    }

    [Fact]
    public async Task Handle_WhenFilteredByBranch_ExcludesShiftsFromOtherBranches()
    {
        // Arrange
        var date = DateTime.Today;
        var shifts = new List<ShiftConciliationDataDto>
        {
            new()
            {
                InternalId = 1,
                IdContpaqi = 1,
                CashierId = 10,
                OpeningTime = date.AddHours(8),
                CashCollected = 5000m
            },
            new()
            {
                InternalId = 2,
                IdContpaqi = 2,
                CashierId = 20,
                OpeningTime = date.AddHours(8),
                CashCollected = 8000m
            }
        };

        var branch1 = SetId(Branch.Create("Sucursal Centro", "SC", "A", "B", "DEV", "Calle 1", null), 1);
        var branch2 = SetId(Branch.Create("Sucursal Norte", "SN", "A", "B", "DEV", "Calle 2", null), 2);

        var boxBranch1 = SetId(CashRegister.Create("Caja 1", "A", 1, "192.168.1.101", 9100, branch1.Id, false, TipoTerminal.PisoVentas), 10);
        var boxBranch2 = SetId(CashRegister.Create("Caja 2", "B", 2, "192.168.1.102", 9100, branch2.Id, false, TipoTerminal.PisoVentas), 20);

        _mockFoxProShiftRepo
            .Setup(r => r.GetTodayShiftsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _mockBranchRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { branch1, branch2 });

        _mockCashRegisterRepo
            .Setup(r => r.GetByBranchAsync(branch1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashRegister> { boxBranch1 });

        _mockCashierRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Cashier>());

        _mockConciliacionRepo
            .Setup(r => r.GetAlreadyConciliatedShiftIdsAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<int>());

        _mockCashCollectionRepo
            .Setup(r => r.GetCollectionsByRegisterAndTimeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CashCollection>());

        var query = new GetMonitorCajasQuery(date, branch1.Id);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert: Solo debe retornar la caja perteneciente a la sucursal 1
        Assert.Single(result);
        Assert.Equal("Caja 1", result[0].CashRegisterName);
        Assert.Equal("Sucursal Centro", result[0].BranchName);
        Assert.Equal(1, result[0].BranchId);
    }
}
