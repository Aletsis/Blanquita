using System;
using System.Collections.Generic;

namespace Blanquita.Application.DTOs;

public class ProductSalesReportDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalUnits { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ProductSalesDetailDto> Details { get; set; } = new();
}

public class ProductSalesDetailDto
{
    public string Folio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Units { get; set; }
    public decimal Price { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
