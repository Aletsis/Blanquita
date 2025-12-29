using Blanquita.Application.DTOs;
using Blanquita.Domain.Entities;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services;

public class PrinterServiceTests
{
    private readonly BlanquitaDbContext _context;
    private readonly Mock<ILogger<PrinterService>> _loggerMock;
    private readonly PrinterService _service;

    public PrinterServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlanquitaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BlanquitaDbContext(options);
        _loggerMock = new Mock<ILogger<PrinterService>>();
        _service = new PrinterService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActive()
    {
        _context.Printers.Add(new Printer { Name = "P1", IpAddress = "1.1.1.1", Port = 9100, IsActive = true });
        _context.Printers.Add(new Printer { Name = "P2", IpAddress = "2.2.2.2", Port = 9100, IsActive = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("P1", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPrinter_WhenExists()
    {
        var printer = new Printer { Name = "Test", IpAddress = "3.3.3.3", Port = 9100, IsActive = true };
        _context.Printers.Add(printer);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(printer.Id);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal("3.3.3.3", result.IpAddress);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddToDb()
    {
        var dto = new PrinterDto { Name = "New", IpAddress = "4.4.4.4", Port = 9100 };

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(0, result.Id);
        Assert.True(result.IsActive);
        
        var inDb = await _context.Printers.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("New", inDb.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyPrinter()
    {
        var printer = new Printer { Name = "Old", IpAddress = "5.5.5.5", Port = 9100, IsActive = true };
        _context.Printers.Add(printer);
        await _context.SaveChangesAsync();

        var dto = new PrinterDto { Id = printer.Id, Name = "Updated", IpAddress = "6.6.6.6", Port = 8080 };
        await _service.UpdateAsync(dto);

        var updated = await _context.Printers.FindAsync(printer.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal("6.6.6.6", updated.IpAddress);
        Assert.Equal(8080, updated.Port);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var dto = new PrinterDto { Id = 999, Name = "Test", IpAddress = "1.1.1.1", Port = 9100 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(dto));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetInactive()
    {
        var printer = new Printer { Name = "ToDelete", IpAddress = "7.7.7.7", Port = 9100, IsActive = true };
        _context.Printers.Add(printer);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(printer.Id);

        var deleted = await _context.Printers.FindAsync(printer.Id);
        Assert.NotNull(deleted);
        Assert.False(deleted.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(999));
    }
}
