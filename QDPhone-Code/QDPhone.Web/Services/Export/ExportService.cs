using ClosedXML.Excel;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Services;

public interface IExportService
{
    byte[] ExportOrdersToExcel(IEnumerable<OrderExportRowViewModel> rows);
}

public class ExportService : IExportService
{
    public byte[] ExportOrdersToExcel(IEnumerable<OrderExportRowViewModel> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Orders");
        ws.Cell(1, 1).Value = "OrderId";
        ws.Cell(1, 2).Value = "UserName";
        ws.Cell(1, 3).Value = "ProductName";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "Total";
        var row = 2;
        foreach (var x in rows)
        {
            ws.Cell(row, 1).Value = x.OrderId;
            ws.Cell(row, 2).Value = x.UserName;
            ws.Cell(row, 3).Value = x.ProductName;
            ws.Cell(row, 4).Value = x.Status;
            ws.Cell(row, 5).Value = x.TotalAmount;
            row++;
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}

