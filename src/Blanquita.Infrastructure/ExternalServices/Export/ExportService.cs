using Blanquita.Application.Interfaces;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Reflection;
using Blanquita.Infrastructure.ExternalServices.Export;
using Blanquita.Domain.Entities;

namespace Blanquita.Infrastructure.ExternalServices.Export;

public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;
    private readonly CultureInfo _culturaMX = new("es-MX");

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Data", CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(sheetName);

                var dataList = data.ToList();
                if (!dataList.Any())
                {
                    _logger.LogWarning("No data to export to Excel");
                    return Array.Empty<byte>();
                }

                // Get properties
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                // Headers
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = properties[i].Name;
                }

                // Style headers
                worksheet.Range(1, 1, 1, properties.Length).Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#424242"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Data rows
                int row = 2;
                foreach (var item in dataList)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var value = properties[col].GetValue(item);
                        
                        if (value != null)
                        {
                            var cell = worksheet.Cell(row, col + 1);
                            
                            // Format based on type
                            if (value is DateTime dateTime)
                            {
                                cell.Value = dateTime;
                                cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                            }
                            else if (value is decimal || value is double || value is float)
                            {
                                cell.Value = Convert.ToDecimal(value);
                                cell.Style.NumberFormat.Format = "$#,##0.00";
                            }
                            else if (value is int || value is long)
                            {
                                cell.Value = Convert.ToInt64(value);
                            }
                            else
                            {
                                cell.Value = value.ToString();
                            }
                        }
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Convert to bytes
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                
                _logger.LogInformation("Excel exported successfully with {RowCount} rows", dataList.Count);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string title = "Report", CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var dataList = data.ToList();
                if (!dataList.Any())
                {
                    _logger.LogWarning("No data to export to PDF");
                    return Array.Empty<byte>();
                }

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter.Landscape());
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        // Header
                        page.Header()
                            .Background(Colors.Blue.Darken2)
                            .Padding(10)
                            .Column(column =>
                            {
                                column.Item().Text(title)
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.White);

                                column.Item().PaddingTop(5).AlignRight()
                                    .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontColor(Colors.White).FontSize(8);
                            });

                        // Content
                        page.Content()
                            .PaddingVertical(5)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (var _ in properties)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                // Headers
                                table.Header(header =>
                                {
                                    foreach (var prop in properties)
                                    {
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).Text(prop.Name)
                                            .FontColor(Colors.White).Bold().FontSize(8);
                                    }
                                });

                                // Data rows
                                foreach (var item in dataList)
                                {
                                    foreach (var prop in properties)
                                    {
                                        var value = prop.GetValue(item);
                                        var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3);

                                        if (value != null)
                                        {
                                            if (value is DateTime dateTime)
                                            {
                                                cell.Text(dateTime.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                                            }
                                            else if (value is decimal || value is double || value is float)
                                            {
                                                cell.AlignRight().Text(Convert.ToDecimal(value).ToString("C2", _culturaMX)).FontSize(8);
                                            }
                                            else
                                            {
                                                cell.Text(value.ToString()).FontSize(8);
                                            }
                                        }
                                        else
                                        {
                                            cell.Text("").FontSize(8);
                                        }
                                    }
                                }
                            });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Página ");
                                text.CurrentPageNumber();
                                text.Span(" de ");
                                text.TotalPages();
                            });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("PDF exported successfully with {RowCount} rows", dataList.Count);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to PDF");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<byte[]> ExportReporteToExcelAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Detalle Reporte");

                // Header Styling
                var labelStyle = workbook.Style.Font.SetBold().Font.SetFontColor(XLColor.Gray);
                
                // Report Header
                worksheet.Cell(1, 1).Value = "DETALLE DE REPORTE HISTÓRICO";
                worksheet.Range(1, 1, 1, 6).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Info Block
                worksheet.Cell(3, 1).Value = "Sucursal:";
                worksheet.Cell(3, 1).Style = labelStyle;
                worksheet.Cell(3, 2).Value = reporte.Sucursal.Nombre;

                worksheet.Cell(3, 4).Value = "Fecha Reporte:";
                worksheet.Cell(3, 4).Style = labelStyle;
                worksheet.Cell(3, 5).Value = reporte.Fecha;
                worksheet.Cell(3, 5).Style.DateFormat.Format = "dd/MM/yyyy";

                worksheet.Cell(4, 1).Value = "Generado:";
                worksheet.Cell(4, 1).Style = labelStyle;
                worksheet.Cell(4, 2).Value = reporte.FechaGeneracion;
                worksheet.Cell(4, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                // Financial Summary
                worksheet.Cell(6, 1).Value = "Total Sistema";
                worksheet.Cell(6, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(7, 1).Value = reporte.TotalSistema;
                worksheet.Cell(7, 1).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Cell(6, 2).Value = "Total Corte";
                worksheet.Cell(6, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(7, 2).Value = reporte.TotalCorteManual;
                worksheet.Cell(7, 2).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Cell(6, 3).Value = "Diferencia";
                worksheet.Cell(6, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(7, 3).Value = reporte.Diferencia;
                worksheet.Cell(7, 3).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(7, 3).Style.Font.SetBold();
                if (reporte.Diferencia > 0) worksheet.Cell(7, 3).Style.Font.SetFontColor(XLColor.Blue);
                else if (reporte.Diferencia < 0) worksheet.Cell(7, 3).Style.Font.SetFontColor(XLColor.Red);
                else worksheet.Cell(7, 3).Style.Font.SetFontColor(XLColor.Green);

                // Notes
                if (!string.IsNullOrEmpty(reporte.Notas))
                {
                    worksheet.Cell(9, 1).Value = "Notas:";
                    worksheet.Cell(9, 1).Style = labelStyle;
                    worksheet.Cell(10, 1).Value = reporte.Notas;
                    worksheet.Range(10, 1, 10, 6).Merge().Style.Alignment.SetWrapText(true);
                }

                // Table Details
                int row = string.IsNullOrEmpty(reporte.Notas) ? 10 : 12;
                
                // Table Headers
                var headers = new[] { "Fecha", "Caja", "Facturado", "Devolución", "Venta Global", "Total" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                }
                
                var headerRange = worksheet.Range(row, 1, row, headers.Length);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f5f5f5"));
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                row++;
                foreach (var item in reporte.Detalles.OrderBy(x => x.Caja))
                {
                    worksheet.Cell(row, 1).Value = item.Fecha;
                    worksheet.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    worksheet.Cell(row, 2).Value = item.Caja;
                    worksheet.Cell(row, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    
                    worksheet.Cell(row, 3).Value = item.Facturado;
                    worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";

                    worksheet.Cell(row, 4).Value = item.Devolucion;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                    if (item.Devolucion > 0) worksheet.Cell(row, 4).Style.Font.SetFontColor(XLColor.Red);

                    worksheet.Cell(row, 5).Value = item.VentaGlobal;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";

                    worksheet.Cell(row, 6).Value = item.Total;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 6).Style.Font.SetBold();

                    row++;
                }

                // AutoFit
                worksheet.Columns().AdjustToContents();
                
                 // Totals Row
                var totalRow = worksheet.Range(row, 1, row, 6);
                totalRow.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f5f5f5"));
                totalRow.Style.Font.SetBold();
                totalRow.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(row, 1).Value = "TOTALES";
                worksheet.Range(row, 1, row, 2).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Cell(row, 3).Value = reporte.Detalles.Sum(x => x.Facturado);
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Cell(row, 4).Value = reporte.Detalles.Sum(x => x.Devolucion);
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 4).Style.Font.SetFontColor(XLColor.Red);

                worksheet.Cell(row, 5).Value = reporte.Detalles.Sum(x => x.VentaGlobal);
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Cell(row, 6).Value = reporte.TotalSistema;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                
                _logger.LogInformation("ReporteHistorico exported to Excel successfully");
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting ReporteHistorico to Excel");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<byte[]> ExportReporteToPdfAsync(ReporteHistorico reporte, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter.Landscape());
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(header => ComposeHeader(header, reporte));
                        page.Content().Element(content => ComposeContent(content, reporte));
                        
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("ReporteHistorico exported to PDF successfully");
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting ReporteHistorico to PDF");
                throw;
            }
        }, cancellationToken);
    }

    private void ComposeHeader(IContainer container, ReporteHistorico reporte)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Reporte Histórico - {reporte.Sucursal.Nombre}")
                    .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);

                column.Item().Text(text =>
                {
                    text.Span("Fecha del Reporte: ").SemiBold();
                    text.Span($"{reporte.Fecha:dd/MM/yyyy}");
                    text.Span("  |  ");
                    text.Span("Generado: ").SemiBold();
                    text.Span($"{reporte.FechaGeneracion:dd/MM/yyyy HH:mm}");
                });
            });
        });
    }

    private void ComposeContent(IContainer container, ReporteHistorico reporte)
    {
        container.PaddingVertical(10).Column(column => 
        {
            // Summary Cards
            column.Item().Row(row => 
            {
                row.RelativeItem().Component(new FinancialCard("Total Sistema", reporte.TotalSistema, Colors.Grey.Darken3));
                row.RelativeItem().Component(new FinancialCard("Total Corte", reporte.TotalCorteManual, Colors.Grey.Darken3));
                
                var diffColor = reporte.Diferencia > 0 ? Colors.Blue.Darken2 : (reporte.Diferencia < 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                row.RelativeItem().Component(new FinancialCard("Diferencia", reporte.Diferencia, diffColor));
            });
            
            if(!string.IsNullOrEmpty(reporte.Notas))
            {
                column.Item().PaddingTop(10).Text(text => 
                {
                    text.Span("Notas: ").Bold();
                    text.Span(reporte.Notas);
                });
            }

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80); // Fecha
                    columns.ConstantColumn(60); // Caja
                    columns.RelativeColumn(); // Facturado
                    columns.RelativeColumn(); // Devolución
                    columns.RelativeColumn(); // Venta Global
                    columns.RelativeColumn(); // Total
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Fecha");
                    header.Cell().Element(CellStyle).Text("Caja");
                    header.Cell().Element(CellStyle).AlignRight().Text("Facturado");
                    header.Cell().Element(CellStyle).AlignRight().Text("Devolución");
                    header.Cell().Element(CellStyle).AlignRight().Text("Venta Global");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).DefaultTextStyle(x => x.SemiBold());
                    }
                });

                foreach (var item in reporte.Detalles.OrderBy(x => x.Caja))
                {
                    table.Cell().Element(CellStyle).Text(item.Fecha);
                    table.Cell().Element(CellStyle).Text(item.Caja).FontColor(Colors.Blue.Darken2);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Facturado.ToString("C2", _culturaMX));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Devolucion.ToString("C2", _culturaMX)).FontColor(item.Devolucion > 0 ? Colors.Red.Darken2 : Colors.Black);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.VentaGlobal.ToString("C2", _culturaMX));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Total.ToString("C2", _culturaMX)).SemiBold();

                     IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                    }
                }
                
                // Footer Totals
                table.Cell().ColumnSpan(2).Element(FooterStyle).Text("TOTALES").SemiBold();
                table.Cell().Element(FooterStyle).AlignRight().Text(reporte.Detalles.Sum(x => x.Facturado).ToString("C2", _culturaMX)).SemiBold();
                table.Cell().Element(FooterStyle).AlignRight().Text(reporte.Detalles.Sum(x => x.Devolucion).ToString("C2", _culturaMX)).FontColor(Colors.Red.Darken2).SemiBold();
                table.Cell().Element(FooterStyle).AlignRight().Text(reporte.Detalles.Sum(x => x.VentaGlobal).ToString("C2", _culturaMX)).SemiBold();
                table.Cell().Element(FooterStyle).AlignRight().Text(reporte.TotalSistema.ToString("C2", _culturaMX)).SemiBold();
                
                 IContainer FooterStyle(IContainer container)
                {
                    return container.Background(Colors.Grey.Lighten4).PaddingVertical(5);
                }
            });
        });
    }
    
    private class FinancialCard : IComponent
    {
        private string Label { get; }
        private decimal Value { get; }
        private string ColorHex { get; }

        public FinancialCard(string label, decimal value, string colorHex)
        {
            Label = label;
            Value = value;
            ColorHex = colorHex;
        }

        public void Compose(IContainer container)
        {
            container.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten3).Background(Colors.Grey.Lighten5).Padding(10).Column(column => 
            {
                column.Item().Text(Label).FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().Text(Value.ToString("C2", new CultureInfo("es-MX"))).FontSize(14).Bold().FontColor(ColorHex);
            });
        }
    }
}
