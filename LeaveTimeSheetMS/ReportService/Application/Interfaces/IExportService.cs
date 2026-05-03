namespace ReportService.Application.Interfaces;

/// <summary>
/// FR-REP-004: Export reports to Excel (.xlsx) and PDF.
/// </summary>
public interface IExportService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName);
    byte[] ExportToPdf<T>(IEnumerable<T> data, string title);
}