using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IDataService
{
    // 分类
    List<Category> GetCategories();
    Category? GetCategoryById(int id);
    void AddCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(int id);

    // 账单
    List<Bill> GetBills(int year, int month, int? categoryId = null);
    Bill? GetBillById(int id);
    void AddBill(Bill bill);
    void UpdateBill(Bill bill);
    void DeleteBill(int id);

    // 月度统计
    List<MonthlySummary> GetMonthlySummary(int year, int month, bool isIncome = false);
    decimal GetMonthlyTotal(int year, int month, bool isIncome = false);

    // 预算
    List<Budget> GetBudgets(string month);
    Budget? GetBudget(int categoryId, string month);
    void SetBudget(Budget budget);
    void DeleteBudget(int id);
}
