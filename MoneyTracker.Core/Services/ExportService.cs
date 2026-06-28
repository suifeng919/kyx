using OfficeOpenXml;
using MoneyTracker.Core.Interfaces;

namespace MoneyTracker.Core.Services;

public class ExportService
{
    private readonly IDataService _data;

    public ExportService(IDataService data)
    {
        _data = data;
    }

    /// <summary>将指定月份的账单导出为Excel文件</summary>
    public void ExportToExcel(int year, int month, string outputPath)
    {
        var bills = _data.GetBills(year, month);
        var categories = _data.GetCategories();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add($"账单_{year}_{month}");

        // 表头
        sheet.Cells[1, 1].Value = "日期";
        sheet.Cells[1, 2].Value = "分类";
        sheet.Cells[1, 3].Value = "类型";
        sheet.Cells[1, 4].Value = "金额";
        sheet.Cells[1, 5].Value = "备注";

        using (var header = sheet.Cells[1, 1, 1, 5])
        {
            header.Style.Font.Bold = true;
            header.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // 数据行
        decimal totalIncome = 0, totalExpense = 0;
        for (int i = 0; i < bills.Count; i++)
        {
            var b = bills[i];
            int row = i + 2;
            sheet.Cells[row, 1].Value = b.Date;
            sheet.Cells[row, 2].Value = b.CategoryName;
            sheet.Cells[row, 3].Value = b.IsIncome ? "收入" : "支出";
            sheet.Cells[row, 4].Value = b.Amount;
            sheet.Cells[row, 5].Value = b.Remark;

            if (b.IsIncome) totalIncome += b.Amount;
            else totalExpense += b.Amount;
        }

        // 汇总行
        int summaryRow = bills.Count + 3;
        sheet.Cells[summaryRow, 1].Value = "收入合计";
        sheet.Cells[summaryRow, 4].Value = totalIncome;
        sheet.Cells[summaryRow, 1, summaryRow, 4].Style.Font.Bold = true;

        sheet.Cells[summaryRow + 1, 1].Value = "支出合计";
        sheet.Cells[summaryRow + 1, 4].Value = totalExpense;
        sheet.Cells[summaryRow + 1, 1, summaryRow + 1, 4].Style.Font.Bold = true;

        sheet.Cells[summaryRow + 2, 1].Value = "结余";
        sheet.Cells[summaryRow + 2, 4].Value = totalIncome - totalExpense;
        sheet.Cells[summaryRow + 2, 1, summaryRow + 2, 4].Style.Font.Bold = true;

        sheet.Cells.AutoFitColumns();
        package.SaveAs(new FileInfo(outputPath));
    }
}
