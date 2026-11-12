using System;
using System.ComponentModel.DataAnnotations;

namespace Blanquita.Application.DTOs;

public class CommercialCompraDto
{
    public int CIDDOCUMENTO { get; set; }
    public string? CSERIEDOCUMENTO { get; set; }
    public double CFOLIO { get; set; }
    public DateTime? CFECHA { get; set; }
    public int CIDCLIENTEPROVEEDOR { get; set; }
    public string? CRAZONSOCIAL { get; set; }
    public string? CRFC { get; set; }
    public string? CREFERENCIA { get; set; }
    public int CCANCELADO { get; set; }
    public double CIMPUESTO1 { get; set; }
    public double CTOTAL { get; set; }
    public double CPENDIENTE { get; set; }
    public string? CTEXTOEXTRA1 { get; set; }
    public DateTime? CFECHAVENCIMIENTO { get; set; }
    public DateTime? CFECHAENTREGARECEPCION { get; set; }
    public string? CGUIDDOCUMENTO { get; set; }
    public string? CCODIGOCONCEPTO { get; set; }
}

public class CommercialMovimientoDto
{
    public int CIDMOVIMIENTO { get; set; }
    public int CIDDOCUMENTO { get; set; }
    public int CIDPRODUCTO { get; set; }
    public double CUNIDADES { get; set; }
    public double CPRECIO { get; set; }
    public double CNETO { get; set; }
    public double CIMPUESTO1 { get; set; }
    public double CTOTAL { get; set; }
    public string? CCODIGOPRODUCTO { get; set; }
    public string? CNOMBREPRODUCTO { get; set; }
}

public class CommercialProveedorDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? RFC { get; set; }
}

public class CommercialProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class CommercialSalidaDto
{
    public int CIDDOCUMENTO { get; set; }
    public string? CSERIEDOCUMENTO { get; set; }
    public double CFOLIO { get; set; }
    public DateTime? CFECHA { get; set; }
    public int CIDCLIENTEPROVEEDOR { get; set; }
    public string? CRAZONSOCIAL { get; set; }
    public string? CRFC { get; set; }
    public string? CREFERENCIA { get; set; }
    public int CCANCELADO { get; set; }
    public double CIMPUESTO1 { get; set; }
    public double CTOTAL { get; set; }
    public double CPENDIENTE { get; set; }
    public string? CTEXTOEXTRA1 { get; set; }
    public DateTime? CFECHAVENCIMIENTO { get; set; }
    public DateTime? CFECHAENTREGARECEPCION { get; set; }
    public string? CGUIDDOCUMENTO { get; set; }
    public string? CCODIGOCONCEPTO { get; set; }
}

public class CommercialConceptoDto
{
    public int CIDCONCEPTO { get; set; }
    public string CCODIGOCONCEPTO { get; set; } = string.Empty;
    public string CNOMBRECONCEPTO { get; set; } = string.Empty;
    public int CIDALMACEN { get; set; }
    public string CCODIGOALMACEN { get; set; } = string.Empty;
    public string CodigoAlmacen { get; set; } = string.Empty; // alias
}


public class CreateCommercialSalidaDto
{
    public string CodigoConcepto { get; set; } = string.Empty;
    public string ConceptoCodigo { get; set; } = string.Empty; // alias
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public List<CreateCommercialSalidaMovimientoDto> Partidas { get; set; } = new();
    public List<CreateCommercialSalidaMovimientoDto> Movimientos { get; set; } = new(); // alias
}

public class CreateCommercialSalidaMovimientoDto
{
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty; // alias
    public double Unidades { get; set; }
    public double Cantidad { get; set; } // alias
    public double Precio { get; set; }
    public string CodigoAlmacen { get; set; } = string.Empty;
}

public class CreateCommercialSalidaResponseDto
{
    public int IdDocumento { get; set; }
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
}

public class CommercialLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? Nivel { get; set; }
}

public class CreateCommercialCompraDto
{
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    public double Folio { get; set; } = 0;

    [Required(ErrorMessage = "El código del proveedor es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del proveedor debe tener entre 1 y 30 caracteres.")]
    public string CodigoProveedor { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string? Referencia { get; set; }

    public string? Observaciones { get; set; }

    [Required(ErrorMessage = "El código del almacén es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;

    public int NumeroMoneda { get; set; } = 1;
    public double TipoCambio { get; set; } = 1.0;

    [MinLength(1, ErrorMessage = "La compra debe tener al menos una partida.")]
    public List<CreateCommercialCompraPartidaDto> Partidas { get; set; } = new();
}

public class CreateCommercialCompraPartidaDto
{
    [Required(ErrorMessage = "El código del producto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio o costo unitario debe ser mayor o igual a 0.")]
    public double PrecioUnitario { get; set; }
}

public class CreateCommercialCompraResponseDto
{
    public int IdDocumento { get; set; }
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
}







