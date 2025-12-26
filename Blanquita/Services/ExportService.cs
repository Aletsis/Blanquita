using Blanquita.Interfaces;
using Blanquita.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using Microsoft.JSInterop;

namespace Blanquita.Services
{
    public class ExportService : IExportService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly CultureInfo _culturaMX = new CultureInfo("es-MX");

        public ExportService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportarExcelAsync(Reporte reporte)
        {
            return await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add($"Reporte {reporte.Sucursal}");

                // Configurar título
                worksheet.Cell("A1").Value = "REPORTE DE FACTURACIÓN";
                worksheet.Range("A1:F1").Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#28B060"))
                    .Font.SetFontColor(XLColor.White);

                // Información del reporte
                worksheet.Cell("A3").Value = "Sucursal:";
                worksheet.Cell("B3").Value = reporte.Sucursal;
                worksheet.Cell("A4").Value = "Fecha:";
                worksheet.Cell("B4").Value = reporte.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell("A5").Value = "Generado:";
                worksheet.Cell("B5").Value = reporte.FechaGeneracion.ToString("dd/MM/yyyy HH:mm:ss");

                // Estilo para la información
                worksheet.Range("A3:A5").Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightGray);

                // Encabezados de la tabla
                int filaInicio = 7;
                worksheet.Cell(filaInicio, 1).Value = "Fecha";
                worksheet.Cell(filaInicio, 2).Value = "Caja";
                worksheet.Cell(filaInicio, 3).Value = "Facturado";
                worksheet.Cell(filaInicio, 4).Value = "Devolución";
                worksheet.Cell(filaInicio, 5).Value = "Venta Global";
                worksheet.Cell(filaInicio, 6).Value = "Total";

                // Estilo de encabezados
                worksheet.Range(filaInicio, 1, filaInicio, 6).Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#424242"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Datos
                int filaActual = filaInicio + 1;
                foreach (var detalle in reporte.Detalles.OrderBy(d => d.Caja))
                {
                    worksheet.Cell(filaActual, 1).Value = detalle.Fecha;
                    worksheet.Cell(filaActual, 2).Value = detalle.Caja;
                    worksheet.Cell(filaActual, 3).Value = detalle.Facturado;
                    worksheet.Cell(filaActual, 4).Value = detalle.Devolucion;
                    worksheet.Cell(filaActual, 5).Value = detalle.VentaGlobal;
                    worksheet.Cell(filaActual, 6).Value = detalle.Total;

                    // Formato moneda
                    worksheet.Range(filaActual, 3, filaActual, 6).Style
                        .NumberFormat.Format = "$#,##0.00";

                    // Color a devoluciones
                    worksheet.Cell(filaActual, 4).Style
                        .Font.SetFontColor(XLColor.Red);

                    filaActual++;
                }

                // Totales
                int filaTotales = filaActual;
                worksheet.Cell(filaTotales, 1).Value = "TOTALES";
                worksheet.Cell(filaTotales, 2).Value = "";
                worksheet.Cell(filaTotales, 3).Value = reporte.Detalles.Sum(d => d.Facturado);
                worksheet.Cell(filaTotales, 4).Value = reporte.Detalles.Sum(d => d.Devolucion);
                worksheet.Cell(filaTotales, 5).Value = reporte.Detalles.Sum(d => d.VentaGlobal);
                worksheet.Cell(filaTotales, 6).Value = reporte.TotalSistema;

                // Estilo totales
                worksheet.Range(filaTotales, 1, filaTotales, 6).Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .NumberFormat.Format = "$#,##0.00";

                // Resumen
                int filaResumen = filaTotales + 2;
                worksheet.Cell(filaResumen, 1).Value = "Total Sistema:";
                worksheet.Cell(filaResumen, 2).Value = reporte.TotalSistema;
                worksheet.Cell(filaResumen + 1, 1).Value = "Total Corte Manual:";
                worksheet.Cell(filaResumen + 1, 2).Value = reporte.TotalCorteManual;
                worksheet.Cell(filaResumen + 2, 1).Value = "Diferencia:";
                worksheet.Cell(filaResumen + 2, 2).Value = reporte.Diferencia;

                // Estilo resumen
                worksheet.Range(filaResumen, 1, filaResumen + 2, 1).Style
                    .Font.SetBold();
                worksheet.Range(filaResumen, 2, filaResumen + 2, 2).Style
                    .NumberFormat.Format = "$#,##0.00";

                // Color de diferencia
                var colorDif = reporte.Diferencia == 0 ? XLColor.Green :
                               reporte.Diferencia > 0 ? XLColor.Blue : XLColor.Orange;
                worksheet.Cell(filaResumen + 2, 2).Style
                    .Font.SetFontColor(colorDif)
                    .Font.SetBold();

                // Notas
                if (!string.IsNullOrWhiteSpace(reporte.Notas))
                {
                    int filaNotas = filaResumen + 4;
                    worksheet.Cell(filaNotas, 1).Value = "Notas:";
                    worksheet.Cell(filaNotas, 1).Style.Font.SetBold();
                    worksheet.Cell(filaNotas + 1, 1).Value = reporte.Notas;
                    worksheet.Range(filaNotas + 1, 1, filaNotas + 1, 6).Merge();
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                // Convertir a bytes
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            });
        }

        public async Task<byte[]> ExportarPDFAsync(Reporte reporte)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Background(Colors.Green.Darken2)
                            .Padding(10)
                            .Column(column =>
                            {
                                column.Item().Text("REPORTE DE FACTURACIÓN")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.White);

                                column.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text($"Sucursal: {reporte.Sucursal} | Fecha: {reporte.Fecha:dd/MM/yyyy}")
                                        .FontColor(Colors.White).FontSize(9);

                                    row.RelativeItem().AlignRight()
                                        .Text($"Generado: {reporte.FechaGeneracion:dd/MM/yyyy HH:mm}")
                                        .FontColor(Colors.White).FontSize(8);
                                });
                            });

                        page.Content()
                            .PaddingVertical(5)
                            .Column(column =>
                            {
                                // Tabla de detalles
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // Encabezados
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).Text("Fecha").FontColor(Colors.White).Bold().FontSize(8);
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).Text("Caja").FontColor(Colors.White).Bold().FontSize(8);
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).AlignRight().Text("Facturado").FontColor(Colors.White).Bold().FontSize(8);
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).AlignRight().Text("Devolución").FontColor(Colors.White).Bold().FontSize(8);
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).AlignRight().Text("Venta Global").FontColor(Colors.White).Bold().FontSize(8);
                                        header.Cell().Background(Colors.Grey.Darken3)
                                            .Padding(3).AlignRight().Text("Total").FontColor(Colors.White).Bold().FontSize(8);
                                    });

                                    // Datos
                                    foreach (var detalle in reporte.Detalles.OrderBy(d => d.Caja))
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).Text(detalle.Fecha).FontSize(8);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).Text(detalle.Caja).FontSize(8);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).AlignRight().Text(detalle.Facturado.ToString("C2", _culturaMX)).FontSize(8);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).AlignRight().Text(detalle.Devolucion.ToString("C2", _culturaMX))
                                            .FontColor(Colors.Red.Medium).FontSize(8);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).AlignRight().Text(detalle.VentaGlobal.ToString("C2", _culturaMX)).FontSize(8);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(3).AlignRight().Text(detalle.Total.ToString("C2", _culturaMX)).Bold().FontSize(8);
                                    }

                                    // Totales
                                    table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(3)
                                        .Text("TOTALES").Bold().FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                        .Text(reporte.Detalles.Sum(d => d.Facturado).ToString("C2", _culturaMX)).Bold().FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                        .Text(reporte.Detalles.Sum(d => d.Devolucion).ToString("C2", _culturaMX)).Bold().FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                        .Text(reporte.Detalles.Sum(d => d.VentaGlobal).ToString("C2", _culturaMX)).Bold().FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                        .Text(reporte.TotalSistema.ToString("C2", _culturaMX)).Bold().FontSize(8);
                                });

                                // Resumen
                                column.Item().PaddingTop(10).Column(summary =>
                                {
                                    summary.Item().Background(Colors.Green.Lighten4).Padding(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Column(col =>
                                            {
                                                col.Item().Text("Total Sistema:").Bold().FontSize(9);
                                                col.Item().PaddingTop(3).Text("Total Corte Manual:").Bold().FontSize(9);
                                                col.Item().PaddingTop(3).Text("Diferencia:").Bold().FontSize(9);
                                            });

                                            row.RelativeItem().Column(col =>
                                            {
                                                col.Item().AlignRight().Text(reporte.TotalSistema.ToString("C2", _culturaMX)).FontSize(9);
                                                col.Item().PaddingTop(3).AlignRight().Text(reporte.TotalCorteManual.ToString("C2", _culturaMX)).FontSize(9);

                                                var colorDif = reporte.Diferencia == 0 ? Colors.Green.Medium :
                                                               reporte.Diferencia > 0 ? Colors.Blue.Medium : Colors.Orange.Medium;
                                                col.Item().PaddingTop(3).AlignRight()
                                                    .Text(reporte.Diferencia.ToString("C2", _culturaMX))
                                                    .FontColor(colorDif).Bold().FontSize(9);
                                            });
                                        });
                                });

                                // Notas
                                if (!string.IsNullOrWhiteSpace(reporte.Notas))
                                {
                                    column.Item().PaddingTop(8).Column(notas =>
                                    {
                                        notas.Item().Text("Notas:").Bold().FontSize(9);
                                        notas.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten1)
                                            .Padding(6).Text(reporte.Notas).FontSize(8);
                                    });
                                }
                            });

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

                return document.GeneratePdf();
            });
        }

        public async Task DescargarArchivoAsync(byte[] contenido, string nombreArchivo, string mimeType)
        {
            await _jsRuntime.InvokeVoidAsync("fileDownloadHelper.downloadFile", nombreArchivo, mimeType, contenido);
        }
    }
}