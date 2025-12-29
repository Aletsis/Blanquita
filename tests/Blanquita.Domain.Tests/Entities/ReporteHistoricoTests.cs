using Blanquita.Domain.Entities;
using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class ReporteHistoricoTests
{
    [Fact]
    public void Crear_DatosValidos_DeberiaCrearReporte()
    {
        var sucursal = Sucursal.Himno;
        var fecha = DateTime.Now;
        var totalSistema = 1000m;
        var totalCorte = 1200m;
        var detalles = new List<DetalleReporte>();

        var reporte = ReporteHistorico.Crear(sucursal, fecha, totalSistema, totalCorte, detalles);

        Assert.NotNull(reporte);
        Assert.Equal(sucursal, reporte.Sucursal);
        Assert.Equal(fecha, reporte.Fecha);
        Assert.Equal(totalSistema, reporte.TotalSistema);
        Assert.Equal(totalCorte, reporte.TotalCorteManual);
        Assert.Empty(reporte.Detalles);
        Assert.Equal(200m, reporte.Diferencia); // 1200 - 1000
    }

    [Fact]
    public void Crear_ArgumentosInvalidos_DeberiaLanzarExcepcion()
    {
        // Null sucursal
        Assert.Throws<ArgumentNullException>(() => ReporteHistorico.Crear(null!, DateTime.Now, 100, 100, new List<DetalleReporte>()));
        
        // Negative TotalSistema
        Assert.Throws<ArgumentException>(() => ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, -1, 100, new List<DetalleReporte>()));
        
        // Negative TotalCorte
        Assert.Throws<ArgumentException>(() => ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, 100, -1, new List<DetalleReporte>()));
        
        // Future date
        Assert.Throws<ArgumentException>(() => ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now.AddDays(1), 100, 100, new List<DetalleReporte>()));
    }

    [Fact]
    public void Diferencia_CalculosCorrectos()
    {
        var reporte = ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, 1000, 1200, new List<DetalleReporte>());
        Assert.Equal(200, reporte.Diferencia);
        Assert.True(reporte.TieneSuperavit());
        Assert.False(reporte.TieneDeficit());
        Assert.True(reporte.TieneDiferencia());
        Assert.Equal(20.0m, reporte.ObtenerPorcentajeDiferencia()); // (200/1000)*100 = 20%

        var reporteDeficit = ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, 1000, 800, new List<DetalleReporte>());
        Assert.Equal(-200, reporteDeficit.Diferencia);
        Assert.False(reporteDeficit.TieneSuperavit());
        Assert.True(reporteDeficit.TieneDeficit());
        Assert.Equal(-20.0m, reporteDeficit.ObtenerPorcentajeDiferencia());
    }

    [Fact]
    public void ActualizarNotas_DeberiaActualizarPropiedad()
    {
        var reporte = ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, 100, 100, new List<DetalleReporte>());
        reporte.ActualizarNotas("Nota de prueba");
        Assert.Equal("Nota de prueba", reporte.Notas);
    }

    [Fact]
    public void AgregarDetalle_DeberiaAgregarALista()
    {
        var reporte = ReporteHistorico.Crear(Sucursal.Himno, DateTime.Now, 100, 100, new List<DetalleReporte>());
        var detalle = DetalleReporte.Crear("2023-01-01", "Caja 1", 100, 0, 0, 100);

        reporte.AgregarDetalle(detalle);

        Assert.Single(reporte.Detalles);
        Assert.Contains(detalle, reporte.Detalles);
        Assert.True(reporte.TieneDetalles());
        Assert.Equal(1, reporte.ObtenerCantidadDetalles());
    }
}
