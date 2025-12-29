using Blanquita.Domain.Entities;
using Blanquita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Persistence.Context;

public class BlanquitaDbContext : IdentityDbContext<ApplicationUser>
{
    public BlanquitaDbContext(DbContextOptions<BlanquitaDbContext> options) : base(options)
    {
    }

    public DbSet<Cashier> Cashiers { get; set; } = null!;
    public DbSet<CashRegister> CashRegisters { get; set; } = null!;
    public DbSet<Supervisor> Supervisors { get; set; } = null!;
    public DbSet<CashCollection> CashCollections { get; set; } = null!;
    public DbSet<CashCut> CashCuts { get; set; } = null!;
    public DbSet<Printer> Printers { get; set; } = null!;
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; } = null!;
    public DbSet<LabelDesign> LabelDesigns { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlanquitaDbContext).Assembly);

        // Value converters for Value Objects
        var branchIdConverter = new ValueConverter<BranchId, int>(
            v => v.Value,
            v => BranchId.Create(v));

        var moneyConverter = new ValueConverter<Money, decimal>(
            v => v.Amount,
            v => Money.Create(v));

        // Configure Cashier
        modelBuilder.Entity<Cashier>(entity =>
        {
            entity.ToTable("Cajeras");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeNumber).HasColumnName("NumNomina").IsRequired();
            entity.Property(e => e.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchId).HasColumnName("Sucursal").HasConversion(branchIdConverter).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("Edo").IsRequired();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
        });

        // Configure CashRegister
        modelBuilder.Entity<CashRegister>(entity =>
        {
            entity.ToTable("Cajas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchId).HasColumnName("Sucursal").HasConversion(branchIdConverter).IsRequired();
            entity.Property(e => e.IsLastRegister).HasColumnName("Ultima").IsRequired();
            
            entity.OwnsOne(e => e.PrinterConfig, printerConfig =>
            {
                printerConfig.Property(p => p.IpAddress).HasColumnName("IpImpresora").IsRequired().HasMaxLength(50);
                printerConfig.Property(p => p.Port).HasColumnName("Port").IsRequired();
            });

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Supervisor
        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.ToTable("Encargadas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchId).HasColumnName("Sucursal").HasConversion(branchIdConverter).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("Edo").IsRequired();
        });

        // Configure CashCollection
        modelBuilder.Entity<CashCollection>(entity =>
        {
            entity.ToTable("Recolecciones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CashRegisterName).HasColumnName("Caja").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CashierName).HasColumnName("Cajera").IsRequired().HasMaxLength(200);
            entity.Property(e => e.SupervisorName).HasColumnName("Encargada").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CollectionDateTime).HasColumnName("FechaHora").IsRequired();
            entity.Property(e => e.Folio).IsRequired();
            entity.Property(e => e.IsForCashCut).HasColumnName("Corte").IsRequired();

            entity.OwnsOne(e => e.Denominations, denominations =>
            {
                denominations.Property(d => d.Thousands).HasColumnName("Mil").IsRequired();
                denominations.Property(d => d.FiveHundreds).HasColumnName("Quinientos").IsRequired();
                denominations.Property(d => d.TwoHundreds).HasColumnName("Doscientos").IsRequired();
                denominations.Property(d => d.Hundreds).HasColumnName("Cien").IsRequired();
                denominations.Property(d => d.Fifties).HasColumnName("Cincuenta").IsRequired();
                denominations.Property(d => d.Twenties).HasColumnName("Veinte").IsRequired();
            });

            // Computed column for total
            entity.Property<decimal>("CantidadTotal")
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql(
                "CAST([Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20 AS decimal(18,2))");

            // entity.HasIndex(e => e.Folio).IsUnique(); // Removed as Folio is now per-register and resets on cut
        });

        // Configure CashCut
        modelBuilder.Entity<CashCut>(entity =>
        {
            entity.ToTable("Cortes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CashRegisterName).HasColumnName("Caja").IsRequired().HasMaxLength(200);
            entity.Property(e => e.SupervisorName).HasColumnName("Encargada").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CashierName).HasColumnName("Cajera").IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchName).HasColumnName("Sucursal").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CutDateTime).HasColumnName("FechaHora").IsRequired();

            entity.OwnsOne(e => e.Totals, totals =>
            {
                totals.Property(t => t.TotalThousands).HasColumnName("TotalM").IsRequired();
                totals.Property(t => t.TotalFiveHundreds).HasColumnName("TotalQ").IsRequired();
                totals.Property(t => t.TotalTwoHundreds).HasColumnName("TotalD").IsRequired();
                totals.Property(t => t.TotalHundreds).HasColumnName("TotalC").IsRequired();
                totals.Property(t => t.TotalFifties).HasColumnName("TotalCi").IsRequired();
                totals.Property(t => t.TotalTwenties).HasColumnName("TotalV").IsRequired();
                totals.OwnsOne(t => t.TotalSlips, slips =>
                {
                    slips.Property(s => s.Amount).HasColumnName("TotalTira").HasColumnType("decimal(18,2)").IsRequired();
                });
                totals.OwnsOne(t => t.TotalCards, cards =>
                {
                    cards.Property(c => c.Amount).HasColumnName("TotalTarjetas").HasColumnType("decimal(18,2)").IsRequired();
                });
            });

            // Computed column for grand total
            entity.Property<decimal>("GranTotal")
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql(
                "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 AS decimal(18,2))");
        });

        // Configure Printer
        modelBuilder.Entity<Printer>(entity =>
        {
            entity.ToTable("Impresoras");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasColumnName("Ip").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Port).HasColumnName("Puerto").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("Activa").IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });
        // Configure SystemConfiguration
        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.ToTable("Configuracion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Pos10041Path).HasMaxLength(500);
            entity.Property(e => e.Pos10042Path).HasMaxLength(500);
            entity.Property(e => e.Mgw10008Path).HasMaxLength(500);
            entity.Property(e => e.Mgw10005Path).HasMaxLength(500);
        });

        // Configure LabelDesign
        modelBuilder.Entity<LabelDesign>(entity =>
        {
            entity.ToTable("DiseñosEtiquetas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.WidthInMm).HasColumnName("AnchoMm").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.HeightInMm).HasColumnName("AltoMm").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.MarginTopInMm).HasColumnName("MargenSuperiorMm").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.MarginLeftInMm).HasColumnName("MargenIzquierdoMm").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Orientation).HasColumnName("Orientacion").IsRequired().HasMaxLength(1);
            entity.Property(e => e.ProductNameFontSize).HasColumnName("TamañoFuenteNombre").IsRequired();
            entity.Property(e => e.ProductCodeFontSize).HasColumnName("TamañoFuenteCodigo").IsRequired();
            entity.Property(e => e.PriceFontSize).HasColumnName("TamañoFuentePrecio").IsRequired();
            entity.Property(e => e.BarcodeHeightInMm).HasColumnName("AlturaCodigoBarrasMm").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.BarcodeWidth).HasColumnName("AnchoCodigoBarras").IsRequired();
            entity.Property(e => e.IsDefault).HasColumnName("EsPredeterminado").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("Activo").IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
