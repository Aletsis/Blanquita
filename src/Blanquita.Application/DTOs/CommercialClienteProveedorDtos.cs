using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blanquita.Application.DTOs;

public class CommercialPaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class CommercialClienteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public int TipoCliente { get; set; } // 1 = Cliente, 2 = Mixto
    public bool EsAmbos => TipoCliente == 2;
    public int ListaPrecio { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public bool Activo { get; set; }
}

public class CommercialDomicilioDto
{
    public int Id { get; set; }
    public int TipoDireccion { get; set; } // 0 = Fiscal, 1 = Envio
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
}

public class CommercialClienteDetalleDto
{
    // Generales
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }
    public int TipoCliente { get; set; }
    public bool EsAmbos => TipoCliente == 2;
    public bool Activo { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }

    // Fiscales
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    // Domicilios
    public CommercialDomicilioDto? DomicilioFiscal { get; set; }
    public List<CommercialDomicilioDto> DomiciliosEnvio { get; set; } = new();

    // Comerciales y Crédito
    public int MonedaId { get; set; }
    public int ListaPrecio { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public bool PermiteVentaCredito { get; set; }
    public bool PermiteExcederCredito { get; set; }
    public double DescuentoProntoPago { get; set; }
    public int DiasProntoPago { get; set; }
    public double InteresMoratorio { get; set; }
    public int DiaPago { get; set; }
    public int DiasRevision { get; set; }

    // Contacto
    public string? Email1 { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoTelefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }
    public int AlmacenId { get; set; }
    public int AgenteVentaId { get; set; }
    public int AgenteCobroId { get; set; }
}

public class CommercialProveedorDetalleDto
{
    // Generales
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }
    public int TipoCliente { get; set; }
    public bool EsAmbos => TipoCliente == 2;
    public bool Activo { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }

    // Fiscales
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    // Domicilios
    public CommercialDomicilioDto? DomicilioFiscal { get; set; }
    public List<CommercialDomicilioDto> DomiciliosEnvio { get; set; } = new();

    // Condiciones de Compra y Crédito
    public int MonedaId { get; set; }
    public double LimiteCreditoProveedor { get; set; }
    public int DiasCreditoProveedor { get; set; }
    public int TiempoEntrega { get; set; }
    public int DiasEmbarqueProveedor { get; set; }
    public double ImpuestoProveedor1 { get; set; }
    public double ImpuestoProveedor2 { get; set; }
    public double ImpuestoProveedor3 { get; set; }
    public double RetencionProveedor1 { get; set; }
    public double RetencionProveedor2 { get; set; }

    // Contacto
    public string? Email1 { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoTelefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }
}

public class CommercialDocumentoSaldoDto
{
    public int DocumentoId { get; set; }
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public double Total { get; set; }
    public double SaldoPendiente { get; set; }
    public int DiasVencido { get; set; }
    public bool Vencido => DiasVencido > 0;
}

public class CommercialSaldoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? RFC { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public double SaldoActual { get; set; }
    public double SaldoVencido { get; set; }
    public double CreditoDisponible { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public int TotalDocumentosVencidos { get; set; }
    public List<CommercialDocumentoSaldoDto> DocumentosPendientes { get; set; } = new();
}

public class CrearCommercialDomicilioDto
{
    [Required(ErrorMessage = "La calle es requerida.")]
    [StringLength(60, ErrorMessage = "La calle no puede exceder 60 caracteres.")]
    public string Calle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número exterior es requerido.")]
    [StringLength(30, ErrorMessage = "El número exterior no puede exceder 30 caracteres.")]
    public string NumeroExterior { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El número interior no puede exceder 30 caracteres.")]
    public string? NumeroInterior { get; set; }

    [Required(ErrorMessage = "La colonia es requerida.")]
    [StringLength(60, ErrorMessage = "La colonia no puede exceder 60 caracteres.")]
    public string Colonia { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "El municipio no puede exceder 60 caracteres.")]
    public string? Municipio { get; set; }

    [StringLength(60, ErrorMessage = "La ciudad no puede exceder 60 caracteres.")]
    public string? Ciudad { get; set; }

    [StringLength(60, ErrorMessage = "El estado no puede exceder 60 caracteres.")]
    public string? Estado { get; set; }

    [StringLength(60, ErrorMessage = "El país no puede exceder 60 caracteres.")]
    public string Pais { get; set; } = "México";

    [Required(ErrorMessage = "El código postal es requerido.")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe constar de 5 dígitos.")]
    public string CodigoPostal { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono no puede exceder 30 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(60, ErrorMessage = "El email no puede exceder 60 caracteres.")]
    public string? Email { get; set; }
}

public class CreateCommercialClienteDto
{
    [Required(ErrorMessage = "El código es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es requerida.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "La razón social debe tener entre 1 y 60 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "El nombre comercial no puede exceder 60 caracteres.")]
    public string? DenominacionComercial { get; set; }

    [Required(ErrorMessage = "El RFC es requerido.")]
    [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", ErrorMessage = "El formato del RFC no es válido.")]
    public string RFC { get; set; } = string.Empty;

    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }

    [StringLength(10, ErrorMessage = "El régimen fiscal no puede exceder 10 caracteres.")]
    public string? RegimenFiscal { get; set; }

    [StringLength(10, ErrorMessage = "El uso de CFDI no puede exceder 10 caracteres.")]
    public string? UsoCFDI { get; set; }

    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    [Range(1, 10, ErrorMessage = "La lista de precios debe ser entre 1 y 10.")]
    public int ListaPrecio { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "El límite de crédito no puede ser negativo.")]
    public double LimiteCredito { get; set; } = 0.0;

    [Range(0, int.MaxValue, ErrorMessage = "Los días de crédito no pueden ser negativos.")]
    public int DiasCredito { get; set; } = 0;

    public bool PermiteVentaCredito { get; set; } = true;
    public bool PermiteExcederCredito { get; set; } = false;

    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }

    public CrearCommercialDomicilioDto? DomicilioFiscal { get; set; }

    /// <summary>
    /// 1 = Solo Cliente, 2 = Ambos (Cliente y Proveedor)
    /// </summary>
    public int TipoCliente { get; set; } = 1;
    public bool EsProveedorTambien { get; set; }
}

public class UpdateCommercialClienteDto
{
    [StringLength(60, ErrorMessage = "La razón social no puede exceder 60 caracteres.")]
    public string? RazonSocial { get; set; }

    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }

    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    public int? ListaPrecio { get; set; }
    public double? LimiteCredito { get; set; }
    public int? DiasCredito { get; set; }
    public bool? PermiteVentaCredito { get; set; }
    public bool? PermiteExcederCredito { get; set; }

    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }

    public bool? Activo { get; set; }

    /// <summary>
    /// 1 = Solo Cliente, 2 = Ambos (Cliente y Proveedor)
    /// </summary>
    public int? TipoCliente { get; set; }
    public bool? EsProveedorTambien { get; set; }

    public CrearCommercialDomicilioDto? DomicilioFiscal { get; set; }
}

public class CreateCommercialProveedorDto
{
    [Required(ErrorMessage = "El código es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es requerida.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "La razón social debe tener entre 1 y 60 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    public string? DenominacionComercial { get; set; }

    [Required(ErrorMessage = "El RFC es requerido.")]
    [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", ErrorMessage = "El formato del RFC no es válido.")]
    public string RFC { get; set; } = string.Empty;

    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }

    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El límite de crédito no puede ser negativo.")]
    public double LimiteCreditoProveedor { get; set; } = 0.0;

    [Range(0, int.MaxValue, ErrorMessage = "Los días de crédito no pueden ser negativos.")]
    public int DiasCreditoProveedor { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo de entrega no puede ser negativo.")]
    public int TiempoEntrega { get; set; } = 0;

    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }

    public CrearCommercialDomicilioDto? DomicilioFiscal { get; set; }

    /// <summary>
    /// 3 = Solo Proveedor, 2 = Ambos (Cliente y Proveedor)
    /// </summary>
    public int TipoCliente { get; set; } = 3;
    public bool EsClienteTambien { get; set; }
}

public class UpdateCommercialProveedorDto
{
    public string? RazonSocial { get; set; }
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }

    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    public double? LimiteCreditoProveedor { get; set; }
    public int? DiasCreditoProveedor { get; set; }
    public int? TiempoEntrega { get; set; }

    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }

    public bool? Activo { get; set; }

    /// <summary>
    /// 3 = Solo Proveedor, 2 = Ambos (Cliente y Proveedor)
    /// </summary>
    public int? TipoCliente { get; set; }
    public bool? EsClienteTambien { get; set; }

    public CrearCommercialDomicilioDto? DomicilioFiscal { get; set; }
}
