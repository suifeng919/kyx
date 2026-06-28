using System.Data.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyTracker.Core.Models;
using MoneyTracker.Data;

namespace MoneyTracker.Tests;

[TestClass]
public class DataServiceTests
{
    private SQLiteConnection _conn = null!;
    private DataService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        // 每个测试独立的内存数据库，互不干扰
        _conn = new SQLiteConnection("Data Source=:memory:");
        _conn.Open();

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL, Icon TEXT, Type INTEGER NOT NULL DEFAULT 0,
                SortOrder INTEGER DEFAULT 0, IsDefault INTEGER DEFAULT 0 );
            CREATE TABLE IF NOT EXISTS Bills (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId INTEGER NOT NULL, Amount DECIMAL(10,2) NOT NULL,
                IsIncome INTEGER NOT NULL DEFAULT 0, Date TEXT NOT NULL,
                Remark TEXT, CreatedAt TEXT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id) );
            CREATE TABLE IF NOT EXISTS Budgets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId INTEGER NOT NULL, Month TEXT NOT NULL,
                Amount DECIMAL(10,2) NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                UNIQUE(CategoryId, Month) );";
        cmd.ExecuteNonQuery();

        // 插入测试数据
        cmd.CommandText = @"INSERT INTO Categories (Name, Type, SortOrder, IsDefault) VALUES
            ('食堂', 0, 1, 1), ('外卖', 0, 2, 1), ('兼职工资', 1, 11, 1);
            INSERT INTO Bills (CategoryId, Amount, IsIncome, Date, CreatedAt) VALUES
            (1, 15.50, 0, '2024-06-01', '2024-06-01 12:00:00'),
            (1, 12.00, 0, '2024-06-05', '2024-06-05 12:00:00'),
            (3, 500.00, 1, '2024-06-10', '2024-06-10 12:00:00');";
        cmd.ExecuteNonQuery();

        _service = new DataService(_conn);
    }

    [TestCleanup]
    public void Cleanup() => _conn.Dispose();

    // ── TC-01: 添加账单后列表里多一条 ─────────────────

    [TestMethod]
    public void AddBill_Should_IncreaseBillCount()
    {
        var newBill = new Bill
        {
            CategoryId = 2,
            Amount = 25.00m,
            Date = "2024-06-15",
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        _service.AddBill(newBill);
        var bills = _service.GetBills(2024, 6);

        Assert.AreEqual(4, bills.Count);
    }

    // ── TC-02: 按月份筛选只返回该月数据 ───────────────

    [TestMethod]
    public void GetBills_ByMonth_Should_FilterCorrectly()
    {
        var juneBills = _service.GetBills(2024, 6);
        var julyBills = _service.GetBills(2024, 7);

        Assert.AreEqual(3, juneBills.Count);
        Assert.AreEqual(0, julyBills.Count);
    }

    // ── TC-03: 删除账单后列表里少一条 ─────────────────

    [TestMethod]
    public void DeleteBill_Should_RemoveFromDatabase()
    {
        var bills = _service.GetBills(2024, 6);
        int idToDelete = bills[0].Id;

        _service.DeleteBill(idToDelete);
        var afterDelete = _service.GetBills(2024, 6);

        Assert.AreEqual(2, afterDelete.Count);
        Assert.IsNull(_service.GetBillById(idToDelete));
    }

    // ── TC-04: 编辑账单后数据正确更新 ─────────────────

    [TestMethod]
    public void UpdateBill_Should_ModifyExistingRecord()
    {
        var bills = _service.GetBills(2024, 6);
        var bill = bills[0];
        bill.Amount = 99.99m;
        bill.Remark = "修改后的备注";

        _service.UpdateBill(bill);
        var updated = _service.GetBillById(bill.Id);

        Assert.IsNotNull(updated);
        Assert.AreEqual(99.99m, updated.Amount);
        Assert.AreEqual("修改后的备注", updated.Remark);
    }

    // ── TC-05: 按分类筛选只返回对应分类 ───────────────

    [TestMethod]
    public void GetBills_ByCategory_Should_FilterCorrectly()
    {
        var cat1Bills = _service.GetBills(2024, 6, categoryId: 1);
        var cat2Bills = _service.GetBills(2024, 6, categoryId: 2);

        Assert.AreEqual(2, cat1Bills.Count);  // 食堂有2条
        Assert.AreEqual(0, cat2Bills.Count);  // 外卖没有
    }
}
