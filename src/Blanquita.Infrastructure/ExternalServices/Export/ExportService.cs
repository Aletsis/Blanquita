using Blanquita.Application.Interfaces;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Reflection;

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
}
