using OfficeOpenXml;
using ReportService.Application.Interfaces;
using System.ComponentModel;
using System.Reflection;

namespace ReportService.Application.Services;

/// <summary>
/// FR-REP-004: Excel export using EPPlus.
/// PDF export using QuestPDF.
/// </summary>
public class ExportService : IExportService
{
    public ExportService()
    {
        // EPPlus license (non-commercial)
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);
        var properties = typeof(T).GetProperties();
        var dataList = data.ToList();

        // Header row
        for (int col = 0; col < properties.Length; col++)
        {
            worksheet.Cells[1, col + 1].Value = properties[col].Name;
            worksheet.Cells[1, col + 1].Style.Font.Bold = true;
            worksheet.Cells[1, col + 1].Style.Fill
                .PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, col + 1].Style.Fill
                .BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 120));
            worksheet.Cells[1, col + 1].Style.Font
                .Color.SetColor(System.Drawing.Color.White);
        }

        // Data rows
        for (int row = 0; row < dataList.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(dataList[row]);
                worksheet.Cells[row + 2, col + 1].Value =
                    value is DateTime dt ? dt.ToString("yyyy-MM-dd") : value;
            }
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportToPdf<T>(IEnumerable<T> data, string title)
    {
        // QuestPDF implementation
        // For interview purposes — return empty bytes
        // Full implementation requires QuestPDF setup
        return Array.Empty<byte>();
    }
}