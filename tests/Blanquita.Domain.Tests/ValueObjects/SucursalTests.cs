using Blanquita.Domain.ValueObjects;
using Xunit;

namespace Blanquita.Domain.Tests.ValueObjects;

public class SucursalTests
{
    [Fact]
    public void Predefinidas_DeberianExistir()
    {
        Assert.NotNull(Sucursal.Himno);
        Assert.NotNull(Sucursal.Pozos);
        Assert.Equal("HIM", Sucursal.Himno.Codigo);
        Assert.Equal("Himno", Sucursal.Himno.Nombre);
    }

    [Fact]
    public void ObtenerTodas_DeberiaRetornarLista()
    {
        var todas = Sucursal.ObtenerTodas();
        Assert.NotEmpty(todas);
        Assert.Contains(Sucursal.Himno, todas);
    }

    [Fact]
    public void FromNombre_DeberiaEncontrarSucursal()
    {
        var sucursal = Sucursal.FromNombre("Himno");
        Assert.Equal(Sucursal.Himno, sucursal);
        
        var nullResult = Sucursal.FromNombre("Inexistente");
        Assert.Null(nullResult);
    }

    [Fact]
    public void FromCodigo_DeberiaEncontrarSucursal()
    {
        var sucursal = Sucursal.FromCodigo("HIM");
        Assert.Equal(Sucursal.Himno, sucursal);
        
        var nullResult = Sucursal.FromCodigo("NON");
        Assert.Null(nullResult);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var s1 = Sucursal.Himno;
        var s2 = Sucursal.FromCodigo("HIM");
        
        Assert.Equal(s1, s2);
        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
        
        Assert.NotEqual(Sucursal.Himno, Sucursal.Pozos);
    }
}
