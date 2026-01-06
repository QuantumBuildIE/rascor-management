using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rascor.Modules.StockManagement.Application.Interfaces;

namespace Rascor.Modules.StockManagement.Infrastructure.Services;

public class ExportService : IExportService
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName, Dictionary<string, Func<T, object>> columns)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Add headers
        var columnIndex = 1;
        foreach (var column in columns.Keys)
        {
            var headerCell = worksheet.Cell(1, columnIndex);
            headerCell.Value = column;
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            headerCell.Style.Font.FontColor = XLColor.White;
            columnIndex++;
        }

        // Add data rows
        var dataList = data.ToList();
        var rowIndex = 2;
        foreach (var item in dataList)
        {
            columnIndex = 1;
            foreach (var columnSelector in columns.Values)
            {
                var cell = worksheet.Cell(rowIndex, columnIndex);
                var value = columnSelector(item);

                if (value != null)
                {
                    // Handle different data types
                    if (value is decimal || value is double || value is float)
                    {
                        cell.Value = Convert.ToDouble(value);

                        // Check if this is a currency column (contains "Price", "Value", "Cost", "Total" in header)
                        var header = columns.Keys.ElementAt(columnIndex - 1);
                        if (header.Contains("Price") || header.Contains("Value") ||
                            header.Contains("Cost") || header.Contains("Total"))
                        {
                            cell.Style.NumberFormat.Format = "€#,##0.00";
                        }
                        else
                        {
                            cell.Style.NumberFormat.Format = "#,##0.00";
                        }
                    }
                    else if (value is DateTime dateTime)
                    {
                        cell.Value = dateTime;
                        cell.Style.NumberFormat.Format = "dd/MM/yyyy";
                    }
                    else if (value is int || value is long)
                    {
                        cell.Value = Convert.ToDouble(value);
                        cell.Style.NumberFormat.Format = "#,##0";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                }

                columnIndex++;
            }
            rowIndex++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Save to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportToPdf<T>(IEnumerable<T> data, string title, Dictionary<string, Func<T, object>> columns)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RASCOR Ireland")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                col.Item().Text(title)
                                    .FontSize(14)
                                    .SemiBold();
                            });

                            row.ConstantItem(150).Column(col =>
                            {
                                col.Item().AlignRight().Text($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(9);
                            });
                        });

                        column.Item().PaddingTop(0.5f, Unit.Centimetre).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        // Define columns
                        var columnCount = columns.Count;
                        table.ColumnsDefinition(columnsDefinition =>
                        {
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnsDefinition.RelativeColumn();
                            }
                        });

                        // Header
                        foreach (var header in columns.Keys)
                        {
                            table.Cell().Element(CellHeaderStyle).Text(header).FontSize(9).SemiBold();
                        }

                        // Data rows
                        var dataList = data.ToList();
                        var isAlternate = false;

                        foreach (var item in dataList)
                        {
                            Func<IContainer, IContainer> rowStyle = isAlternate ? CellAlternateStyle : CellStyle;

                            foreach (var columnSelector in columns.Values)
                            {
                                var value = columnSelector(item);
                                var formattedValue = FormatValue(value);

                                // Check if this is a numeric column for right alignment
                                var isNumeric = value is decimal or double or float or int or long;

                                var cell = table.Cell().Element(rowStyle);

                                if (isNumeric)
                                {
                                    cell.AlignRight().Text(formattedValue).FontSize(9);
                                }
                                else
                                {
                                    cell.Text(formattedValue).FontSize(9);
                                }
                            }

                            isAlternate = !isAlternate;
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellHeaderStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Background(Colors.Blue.Darken2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontColor(Colors.White));
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    private static IContainer CellAlternateStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten3)
            .Padding(5);
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        return value switch
        {
            decimal d => d.ToString("C2"),
            double db => db.ToString("C2"),
            float f => f.ToString("C2"),
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            int i => i.ToString("N0"),
            long l => l.ToString("N0"),
            _ => value.ToString() ?? string.Empty
        };
    }
}
