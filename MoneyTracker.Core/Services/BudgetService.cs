using MoneyTracker.Core.Interfaces;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Services;

public class BudgetService
{
    private readonly IDataService _data;

    public BudgetService(IDataService data)
    {
        _data = data;
    }

    /// <summary>判断某分类在某月是否超支</summary>
    public bool IsOverBudget(int categoryId, string month)
    {
        var budget = _data.GetBudget(categoryId, month);
        if (budget == null) return false;

        // 从month字符串解析年月
        var parts = month.Split('-');
        int year = int.Parse(parts[0]);
        int monthNum = int.Parse(parts[1]);

        decimal spent = _data.GetMonthlyTotal(year, monthNum, isIncome: false);

        // 只查该分类的支出
        var summaries = _data.GetMonthlySummary(year, monthNum);
        var catSummary = summaries.FirstOrDefault(s => s.CategoryId == categoryId);
        decimal catSpent = catSummary?.TotalAmount ?? 0;

        return catSpent > budget.Amount;
    }

    /// <summary>获取带支出对比的预算列表</summary>
    public List<(Budget Budget, decimal Spent, bool IsOver)> GetBudgetStatusList(string month)
    {
        var budgets = _data.GetBudgets(month);
        var parts = month.Split('-');
        int year = int.Parse(parts[0]);
        int monthNum = int.Parse(parts[1]);
        var summaries = _data.GetMonthlySummary(year, monthNum);

        var result = new List<(Budget, decimal, bool)>();
        foreach (var b in budgets)
        {
            decimal spent = summaries.Where(s => s.CategoryId == b.CategoryId)
                                     .Sum(s => s.TotalAmount);
            result.Add((b, spent, spent > b.Amount));
        }
        return result;
    }
}
