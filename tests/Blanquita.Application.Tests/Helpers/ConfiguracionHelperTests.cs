using Blanquita.Application.DTOs;
using Blanquita.Application.Helpers;
using Blanquita.Domain.Enums;
using Xunit;

namespace Blanquita.Application.Tests.Helpers;

public class ConfiguracionHelperTests
{
    private ConfiguracionDto CreateCompleteConfig()
    {
        return new ConfiguracionDto
        {
            Pos10041Path = "path/to/pos1.dbf",
            Pos10042Path = "path/to/pos2.dbf",
            Mgw10008Path = "path/to/mgw8.dbf",
            Mgw10005Path = "path/to/mgw5.dbf",
            Mgw10045Path = "path/to/mgw45.dbf",
            Mgw10002Path = "path/to/mgw2.dbf",
            Mgw10011Path = "path/to/mgw11.dbf"
        };
    }

    [Fact]
    public void ObtenerRutaPorTipo_ShouldReturnCorrectPath()
    {
        var config = CreateCompleteConfig();

        Assert.Equal("path/to/pos1.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Pos10041));
        Assert.Equal("path/to/pos2.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Pos10042));
        Assert.Equal("path/to/mgw8.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Mgw10008));

        Assert.Equal("path/to/mgw5.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Mgw10005));
        Assert.Equal("path/to/mgw45.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Mgw10045));
        Assert.Equal("path/to/mgw2.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Mgw10002));
        Assert.Equal("path/to/mgw11.dbf", config.ObtenerRutaPorTipo(TipoArchivoDbf.Mgw10011));
    }

    [Fact]
    public void EstablecerRutaPorTipo_ShouldUpdateCorrectPath()
    {
        var config = new ConfiguracionDto();

        config.EstablecerRutaPorTipo(TipoArchivoDbf.Pos10041, "new/path.dbf");
        Assert.Equal("new/path.dbf", config.Pos10041Path);
        
        config.EstablecerRutaPorTipo(TipoArchivoDbf.Pos10042, "new/path2.dbf");
        Assert.Equal("new/path2.dbf", config.Pos10042Path);

        config.EstablecerRutaPorTipo(TipoArchivoDbf.Mgw10002, "new/path3.dbf");
        Assert.Equal("new/path3.dbf", config.Mgw10002Path);
    }

    [Fact]
    public void ObtenerNombreArchivoPorTipo_ShouldReturnCorrectFilename()
    {
        Assert.Equal("POS10041.DBF", ConfiguracionHelper.ObtenerNombreArchivoPorTipo(TipoArchivoDbf.Pos10041));
        Assert.Equal("MGW10008.DBF", ConfiguracionHelper.ObtenerNombreArchivoPorTipo(TipoArchivoDbf.Mgw10008));
        Assert.Equal("MGW10002.DBF", ConfiguracionHelper.ObtenerNombreArchivoPorTipo(TipoArchivoDbf.Mgw10002));
        Assert.Equal("MGW10011.DBF", ConfiguracionHelper.ObtenerNombreArchivoPorTipo(TipoArchivoDbf.Mgw10011));
    }

    [Fact]
    public void TieneTodasLasRutasConfiguradas_ShouldReturnTrue_WhenAllSet()
    {
        var config = CreateCompleteConfig();
        Assert.True(config.TieneTodasLasRutasConfiguradas());
    }

    [Fact]
    public void TieneTodasLasRutasConfiguradas_ShouldReturnFalse_WhenMissing()
    {
        var config = new ConfiguracionDto { Pos10041Path = "set" };
        Assert.False(config.TieneTodasLasRutasConfiguradas());
    }

    [Fact]
    public void ObtenerRutasFaltantes_ShouldReturnMissingOnes()
    {
        var config = new ConfiguracionDto { Pos10041Path = "set" };
        
        var missing = config.ObtenerRutasFaltantes();
        
        Assert.Contains(TipoArchivoDbf.Pos10042, missing);
        Assert.Contains(TipoArchivoDbf.Mgw10008, missing);

        Assert.Contains(TipoArchivoDbf.Mgw10005, missing);
        Assert.Contains(TipoArchivoDbf.Mgw10045, missing);
        Assert.Contains(TipoArchivoDbf.Mgw10002, missing);
        Assert.Contains(TipoArchivoDbf.Mgw10011, missing);
        Assert.DoesNotContain(TipoArchivoDbf.Pos10041, missing);
    }
}
