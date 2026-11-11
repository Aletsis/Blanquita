using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Application.Services;
using Blanquita.Application.Tests.Helpers;
using Blanquita.Domain.Entities;
using Blanquita.Domain.Enums;
using Blanquita.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Application.Tests.Services;

public class ConciliacionServiceTests
{
    private readonly Mock<IFoxProShiftRepository> _shiftRepo = new();
    private readonly Mock<ICashRegisterRepository> _cashRegisterRepo = new();
    private readonly Mock<ICashierRepository> _cashierRepo = new();
    private readonly Mock<ICashCollectionRepository> _collectionRepo = new();
    private readonly Mock<IBranchRepository> _branchRepo = new();
    private readonly Mock<IConciliacionCorteRepository> _conciliacionRepo = new();
    private readonly Mock<IConfiguracionService> _configService = new();
    private readonly ConciliacionService _service;

    public ConciliacionServiceTests()
    {
        _service = new ConciliacionService(
            _shiftRepo.Object,
            _cashRegisterRepo.Object,
            _cashierRepo.Object,
            _collectionRepo.Object,
            _branchRepo.Object,
            _conciliacionRepo.Object,
            _configService.Object,
            new Mock<ILogger<ConciliacionService>>().Object,
            new PassThroughUnitOfWork());
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public async Task PermiteSalidasEfectivoAsync_RequiresBothBranchAndRegisterEnabled(
        bool branchEnabled, bool registerEnabled, bool expected)
    {
        var branch = Branch.Create("Sucursal 1", "01", "C", "G", "D", "", null, branchEnabled);
        var register = CashRegister.Create("Caja 1", "S1", 1, "127.0.0.1", 9100, 10, false, TipoTerminal.PisoVentas, registerEnabled);

        _cashRegisterRepo.Setup(r => r.GetByNameAsync("Caja 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(register);
        _branchRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var result = await _service.PermiteSalidasEfectivoAsync("Caja 1");

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetConciliacionAsync_PopulatesPermiteSalidasEfectivoFromBranchAndRegister()
    {
        var branch = Branch.Create("Sucursal 1", "01", "C", "G", "D", "", null, true);
        var register = CashRegister.Create("Caja 1", "S1", 1, "127.0.0.1", 9100, 10, false, TipoTerminal.PisoVentas, true);

        _cashRegisterRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(register);
        _branchRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _shiftRepo.Setup(r => r.GetShiftDataAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftConciliationDataDto { InternalId = 99, Status = 1, OpeningTime = DateTime.Today });
        _collectionRepo.Setup(r => r.GetCollectionsByRegisterAndTimeAsync("Caja 1", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CashCollection>());

        var result = await _service.GetConciliacionAsync(5, 99, 0, DateTime.Today);

        Assert.True(result.PermiteSalidasEfectivo);
    }
}
