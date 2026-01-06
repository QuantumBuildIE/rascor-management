namespace Rascor.Modules.StockManagement.Application.Interfaces;

public interface IExportService
{
    /// <summary>
    /// Exports data to Excel format (.xlsx)
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="sheetName">Name of the Excel sheet</param>
    /// <param name="columns">Dictionary of column headers and data selectors</param>
    /// <returns>Excel file as byte array</returns>
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName, Dictionary<string, Func<T, object>> columns);

    /// <summary>
    /// Exports data to PDF format
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Data collection to export</param>
    /// <param name="title">Report title</param>
    /// <param name="columns">Dictionary of column headers and data selectors</param>
    /// <returns>PDF file as byte array</returns>
    byte[] ExportToPdf<T>(IEnumerable<T> data, string title, Dictionary<string, Func<T, object>> columns);
}
