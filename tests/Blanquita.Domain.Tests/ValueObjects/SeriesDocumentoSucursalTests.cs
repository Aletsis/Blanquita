using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class SeriesDocumentoSucursalTests
{
    [Fact]
    public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForHimno()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);

        Assert.Equal("COH", series.SerieCliente);
        Assert.Equal("FGIH", series.SerieGlobal);
        Assert.Equal("DFCH", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForPozos()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Pozos);

        Assert.Equal("COP", series.SerieCliente);
        Assert.Equal("FGIP", series.SerieGlobal);
        Assert.Equal("DFCP", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForSoledad()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Soledad);

        Assert.Equal("COS", series.SerieCliente);
        Assert.Equal("FGIS", series.SerieGlobal);
        Assert.Equal("DFCS", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForSaucito()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Saucito);

        Assert.Equal("COFS", series.SerieCliente);
        Assert.Equal("FGIFS", series.SerieGlobal);
        Assert.Equal("DFCFS", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForChapultepec()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Chapultepec);

        Assert.Equal("COX", series.SerieCliente);
        Assert.Equal("FXIS", series.SerieGlobal);
        Assert.Equal("DFCX", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorSucursal_ShouldThrow_WhenSucursalIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            SeriesDocumentoSucursal.ObtenerPorSucursal(null!));
    }

    [Fact]
    public void ObtenerPorNombre_ShouldReturnCorrectSeries_ForValidName()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorNombre("Himno");

        Assert.Equal("COH", series.SerieCliente);
        Assert.Equal("FGIH", series.SerieGlobal);
        Assert.Equal("DFCH", series.SerieDevolucion);
    }

    [Fact]
    public void ObtenerPorNombre_ShouldThrow_WhenNameNotFound()
    {
        Assert.Throws<ArgumentException>(() => 
            SeriesDocumentoSucursal.ObtenerPorNombre("NonExistent"));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSeriesAreEqual()
    {
        var series1 = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);
        var series2 = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);

        Assert.Equal(series1, series2);
        Assert.True(series1 == series2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSeriesAreDifferent()
    {
        var series1 = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);
        var series2 = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Pozos);

        Assert.NotEqual(series1, series2);
        Assert.True(series1 != series2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);

        var result = series.ToString();

        Assert.Contains("COH", result);
        Assert.Contains("FGIH", result);
        Assert.Contains("DFCH", result);
    }
}
