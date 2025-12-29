using Blanquita.Domain.Entities;
using Xunit;

namespace Blanquita.Domain.Tests.Entities;

public class DetalleReporteTests
{
    [Fact]
    public void Crear_Valido_DeberiaCrearDetalle()
    {
        var detalle = DetalleReporte.Crear("2023-01-01", "Caja 1", 1000m, 100m, 200m, 1100m);
        
        Assert.NotNull(detalle);
        Assert.Equal("2023-01-01", detalle.Fecha);
        Assert.Equal("Caja 1", detalle.Caja);
        Assert.Equal(1000m, detalle.Facturado);
        Assert.Equal(100m, detalle.Devolucion);
        Assert.Equal(200m, detalle.VentaGlobal);
        Assert.Equal(1100m, detalle.Total);
    }

    [Fact]
    public void Crear_Invalido_DeberiaLanzarExcepcion()
    {
        Assert.Throws<ArgumentException>(() => DetalleReporte.Crear("", "Caja 1", 0,0,0,0));
        Assert.Throws<ArgumentException>(() => DetalleReporte.Crear("Fecha", "", 0,0,0,0));
    }

    [Fact]
    public void CalcularTotalNeto_DeberiaCalcularCorrectamente()
    {
        // Facturado - Devolucion + VentaGlobal
        // 1000 - 100 + 200 = 1100
        var detalle = DetalleReporte.Crear("F", "C", 1000m, 100m, 200m, 0); 
        
        Assert.Equal(1100m, detalle.CalcularTotalNeto());
    }

    [Fact]
    public void TieneDevoluciones_DeberiaRetornarTrue_SiHay()
    {
        var detalle = DetalleReporte.Crear("F", "C", 1000m, 100m, 0, 0);
        Assert.True(detalle.TieneDevoluciones());
        
        var detalleSin = DetalleReporte.Crear("F", "C", 1000m, 0, 0, 0);
        Assert.False(detalleSin.TieneDevoluciones());
    }

    [Fact]
    public void TieneVentaGlobal_DeberiaRetornarTrue_SiHay()
    {
        var detalle = DetalleReporte.Crear("F", "C", 1000m, 0, 100m, 0);
        Assert.True(detalle.TieneVentaGlobal());
        
        var detalleSin = DetalleReporte.Crear("F", "C", 1000m, 0, 0, 0);
        Assert.False(detalleSin.TieneVentaGlobal());
    }
}
