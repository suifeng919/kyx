using MoneyTracker.Core.Interfaces;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Services;

public class BillService
{
    private readonly IDataService _data;

    public BillService(IDataService data)
    {
        _data = data;
    }

    /// <summary>计算指定分类的月度总支出（纯内存计算，用于测试）</summary>
    public decimal CalculateCategoryTotal(IEnumerable<Bill> bills, int categoryId)
    {
        return bills.Where(b => b.CategoryId == categoryId && !b.IsIncome)
                    .Sum(b => b.Amount);
    }

    /// <summary>计算指定分类的月度总收入（纯内存计算，用于测试）</summary>
    public decimal CalculateCategoryIncome(IEnumerable<Bill> bills, int categoryId)
    {
        return bills.Where(b => b.CategoryId == categoryId && b.IsIncome)
                    .Sum(b => b.Amount);
    }

    /// <summary>从数据源获取月度统计，并计算百分比</summary>
    public List<MonthlySummary> GetSummaryWithPercentages(int year, int month, bool isIncome = false)
    {
        var items = _data.GetMonthlySummary(year, month, isIncome);
        decimal total = items.Sum(i => i.TotalAmount);

        foreach (var item in items)
        {
            item.Percentage = total > 0 ? Math.Round(item.TotalAmount / total * 100, 1) : 0;
        }

        return items;
    }
}
