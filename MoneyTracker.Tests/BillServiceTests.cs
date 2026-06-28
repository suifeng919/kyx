using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyTracker.Core.Models;
using MoneyTracker.Core.Services;

namespace MoneyTracker.Tests;

[TestClass]
public class BillServiceTests
{
    private BillService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        // 注入null IDataService，因为测试只用的纯内存方法
        _service = new BillService(null!);
    }

    // ── TC-06: 月度各分类支出汇总正确 ────────────────

    [TestMethod]
    public void CalculateCategoryTotal_Should_SumByCategory()
    {
        // Arrange
        var bills = new List<Bill>
        {
            new() { CategoryId = 1, Amount = 15.50m },
            new() { CategoryId = 1, Amount = 12.00m },
            new() { CategoryId = 2, Amount = 25.00m },
            new() { CategoryId = 1, Amount = 8.50m },
        };

        // Act
        decimal cat1Total = _service.CalculateCategoryTotal(bills, 1);
        decimal cat2Total = _service.CalculateCategoryTotal(bills, 2);

        // Assert
        Assert.AreEqual(36.00m, cat1Total);   // 15.5 + 12 + 8.5
        Assert.AreEqual(25.00m, cat2Total);
    }

    // ── TC-07: 收入与支出分别计算 ─────────────────────

    [TestMethod]
    public void CalculateCategoryTotal_Should_ExcludeIncome()
    {
        // Arrange
        var bills = new List<Bill>
        {
            new() { CategoryId = 1, Amount = 100m, IsIncome = false },
            new() { CategoryId = 1, Amount = 500m, IsIncome = true  }, // 收入，不应计入支出
            new() { CategoryId = 1, Amount = 50m,  IsIncome = false },
        };

        // Act
        decimal expense = _service.CalculateCategoryTotal(bills, 1);
        decimal income = _service.CalculateCategoryIncome(bills, 1);

        // Assert
        Assert.AreEqual(150m, expense);  // 100 + 50
        Assert.AreEqual(500m, income);
    }

    // ── TC-08: 总金额等于各分类合计 ───────────────────
    // 逻辑：所有分类的支出汇总应等于总支出
    // 注：此处测试的是BillService.CalculateCategoryTotal的正确性，
    // 不涉及数据库的SUM计算

    [TestMethod]
    public void CategoryTotals_Should_MatchOverallSum()
    {
        // Arrange
        var bills = new List<Bill>
        {
            new() { CategoryId = 1, Amount = 10m },
            new() { CategoryId = 2, Amount = 20m },
            new() { CategoryId = 3, Amount = 30m },
        };

        // Act
        decimal total = bills.Where(b => !b.IsIncome).Sum(b => b.Amount);
        decimal cat1 = _service.CalculateCategoryTotal(bills, 1);
        decimal cat2 = _service.CalculateCategoryTotal(bills, 2);
        decimal cat3 = _service.CalculateCategoryTotal(bills, 3);

        // Assert
        Assert.AreEqual(60m, total);
        Assert.AreEqual(cat1 + cat2 + cat3, total);
    }

    // ── TC-09: 空列表时返回0 ─────────────────────────

    [TestMethod]
    public void CalculateCategoryTotal_EmptyList_Should_ReturnZero()
    {
        // Arrange
        var bills = new List<Bill>();

        // Act
        decimal total = _service.CalculateCategoryTotal(bills, 1);

        // Assert
        Assert.AreEqual(0m, total);
    }
}
