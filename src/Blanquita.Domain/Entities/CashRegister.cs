using Blanquita.Domain.ValueObjects;
using Blanquita.Domain.Enums;

namespace Blanquita.Domain.Entities;

/// <summary>
/// Represents a cash register (point of sale) with printer configuration
/// </summary>
public class CashRegister : BaseEntity
{
    public string Name { get; private set; }
    public string Serie { get; private set; }
    public int IdContpaqi { get; private set; }
    public PrinterConfiguration PrinterConfig { get; private set; }
    public BranchId BranchId { get; private set; }
    public TipoTerminal Tipo { get; private set; }
    public bool IsLastRegister { get; private set; }

    // EF Core constructor
    private CashRegister() { }

    private CashRegister(string name, string serie, int idContpaqi, PrinterConfiguration printerConfig, BranchId branchId, TipoTerminal tipo, bool isLastRegister = false)
    {
        Name = name;
        Serie = serie;
        IdContpaqi = idContpaqi;
        PrinterConfig = printerConfig;
        BranchId = branchId;
        Tipo = tipo;
        IsLastRegister = isLastRegister;
    }

    public static CashRegister Create(string name, string serie, int idContpaqi, string printerIp, int printerPort, int branchId, bool isLastRegister = false, TipoTerminal tipo = TipoTerminal.PisoVentas)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        var printerConfig = PrinterConfiguration.Create(printerIp, printerPort);
        var branch = BranchId.Create(branchId);

        return new CashRegister(name, serie ?? string.Empty, idContpaqi, printerConfig, branch, tipo, isLastRegister);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }

    public void UpdateSerie(string serie)
    {
        Serie = serie ?? string.Empty;
    }

    public void UpdateIdContpaqi(int idContpaqi)
    {
        IdContpaqi = idContpaqi;
    }

    public void UpdatePrinterConfiguration(string printerIp, int printerPort)
    {
        PrinterConfig = PrinterConfiguration.Create(printerIp, printerPort);
    }

    public void UpdateBranch(int branchId)
    {
        BranchId = BranchId.Create(branchId);
    }

    public void UpdateTipo(TipoTerminal tipo)
    {
        Tipo = tipo;
    }

    public void SetAsLastRegister()
    {
        IsLastRegister = true;
    }

    public void UnsetAsLastRegister()
    {
        IsLastRegister = false;
    }
}

