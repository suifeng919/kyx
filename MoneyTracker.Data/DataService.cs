using Dapper;
using System.Data.SQLite;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Data;

public class DataService : IDataService
{
    private readonly string? _connectionString;
    private readonly SQLiteConnection? _existingConnection;
    private readonly bool _ownsConnection;

    public DataService(string connectionString)
    {
        _connectionString = connectionString;
        _ownsConnection = true;
    }

    /// <summary>用于测试：接受已有连接的构造方法（内存SQLite）</summary>
    public DataService(SQLiteConnection existingConnection)
    {
        _existingConnection = existingConnection;
        _ownsConnection = false;
    }

    private SQLiteConnection GetConnection()
    {
        if (_existingConnection != null)
            return _existingConnection;
        var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private void ReleaseConnection(SQLiteConnection conn)
    {
        if (_ownsConnection) conn.Dispose();
    }

    // ── 分类 ──────────────────────────────────────────

    public List<Category> GetCategories()
    {
        var conn = GetConnection();
        try { return conn.Query<Category>("SELECT * FROM Categories ORDER BY SortOrder").ToList(); }
        finally { ReleaseConnection(conn); }
    }

    public Category? GetCategoryById(int id)
    {
        var conn = GetConnection();
        try { return conn.QuerySingleOrDefault<Category>("SELECT * FROM Categories WHERE Id = @id", new { id }); }
        finally { ReleaseConnection(conn); }
    }

    public void AddCategory(Category category)
    {
        var conn = GetConnection();
        try { conn.Execute("INSERT INTO Categories (Name, Icon, Type, SortOrder, IsDefault) VALUES (@Name, @Icon, @Type, @SortOrder, @IsDefault)", category); }
        finally { ReleaseConnection(conn); }
    }

    public void UpdateCategory(Category category)
    {
        var conn = GetConnection();
        try { conn.Execute("UPDATE Categories SET Name=@Name, Icon=@Icon, Type=@Type, SortOrder=@SortOrder WHERE Id=@Id", category); }
        finally { ReleaseConnection(conn); }
    }

    public void DeleteCategory(int id)
    {
        var conn = GetConnection();
        try { conn.Execute("DELETE FROM Categories WHERE Id=@id", new { id }); }
        finally { ReleaseConnection(conn); }
    }

    // ── 账单 ──────────────────────────────────────────

    public List<Bill> GetBills(int year, int month, int? categoryId = null)
    {
        var conn = GetConnection();
        try
        {
            var sql = @"SELECT b.*, c.Name AS CategoryName
                        FROM Bills b
                        LEFT JOIN Categories c ON b.CategoryId = c.Id
                        WHERE strftime('%Y', b.Date) = @yearStr
                          AND strftime('%m', b.Date) = @monthStr";

            if (categoryId.HasValue)
                sql += " AND b.CategoryId = @catId";

            sql += " ORDER BY b.Date DESC, b.Id DESC";

            return conn.Query<Bill>(sql, new
            {
                yearStr = year.ToString(),
                monthStr = month.ToString("D2"),
                catId = categoryId
            }).ToList();
        }
        finally { ReleaseConnection(conn); }
    }

    public Bill? GetBillById(int id)
    {
        var conn = GetConnection();
        try
        {
            return conn.QuerySingleOrDefault<Bill>(
                "SELECT b.*, c.Name AS CategoryName FROM Bills b LEFT JOIN Categories c ON b.CategoryId = c.Id WHERE b.Id = @id",
                new { id });
        }
        finally { ReleaseConnection(conn); }
    }

    public void AddBill(Bill bill)
    {
        var conn = GetConnection();
        try
        {
            conn.Execute(
                @"INSERT INTO Bills (CategoryId, Amount, IsIncome, Date, Remark, CreatedAt)
                  VALUES (@CategoryId, @Amount, @IsIncome, @Date, @Remark, @CreatedAt)",
                bill);
        }
        finally { ReleaseConnection(conn); }
    }

    public void UpdateBill(Bill bill)
    {
        var conn = GetConnection();
        try
        {
            conn.Execute(
                @"UPDATE Bills SET CategoryId=@CategoryId, Amount=@Amount, IsIncome=@IsIncome,
                                   Date=@Date, Remark=@Remark WHERE Id=@Id",
                bill);
        }
        finally { ReleaseConnection(conn); }
    }

    public void DeleteBill(int id)
    {
        var conn = GetConnection();
        try { conn.Execute("DELETE FROM Bills WHERE Id=@id", new { id }); }
        finally { ReleaseConnection(conn); }
    }

    // ── 月度统计 ──────────────────────────────────────

    public List<MonthlySummary> GetMonthlySummary(int year, int month, bool isIncome = false)
    {
        var conn = GetConnection();
        try
        {
            return conn.Query<MonthlySummary>(@"
                SELECT c.Name AS CategoryName, c.Id AS CategoryId,
                       SUM(b.Amount) AS TotalAmount, COUNT(*) AS Count
                FROM Bills b
                JOIN Categories c ON b.CategoryId = c.Id
                WHERE strftime('%Y', b.Date) = @yearStr
                  AND strftime('%m', b.Date) = @monthStr
                  AND b.IsIncome = @isInc
                GROUP BY b.CategoryId
                ORDER BY TotalAmount DESC", new
            {
                yearStr = year.ToString(),
                monthStr = month.ToString("D2"),
                isInc = isIncome ? 1 : 0
            }).ToList();
        }
        finally { ReleaseConnection(conn); }
    }

    public decimal GetMonthlyTotal(int year, int month, bool isIncome = false)
    {
        var conn = GetConnection();
        try
        {
            return conn.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(Amount), 0) FROM Bills
                WHERE strftime('%Y', Date) = @yearStr
                  AND strftime('%m', Date) = @monthStr
                  AND IsIncome = @isInc", new
            {
                yearStr = year.ToString(),
                monthStr = month.ToString("D2"),
                isInc = isIncome ? 1 : 0
            });
        }
        finally { ReleaseConnection(conn); }
    }

    // ── 预算 ──────────────────────────────────────────

    public List<Budget> GetBudgets(string month)
    {
        var conn = GetConnection();
        try
        {
            return conn.Query<Budget>(@"
                SELECT b.*, c.Name AS CategoryName
                FROM Budgets b
                JOIN Categories c ON b.CategoryId = c.Id
                WHERE b.Month = @month
                ORDER BY c.SortOrder", new { month }).ToList();
        }
        finally { ReleaseConnection(conn); }
    }

    public Budget? GetBudget(int categoryId, string month)
    {
        var conn = GetConnection();
        try
        {
            return conn.QuerySingleOrDefault<Budget>(
                "SELECT * FROM Budgets WHERE CategoryId = @categoryId AND Month = @month",
                new { categoryId, month });
        }
        finally { ReleaseConnection(conn); }
    }

    public void SetBudget(Budget budget)
    {
        var conn = GetConnection();
        try
        {
            conn.Execute(@"
                INSERT INTO Budgets (CategoryId, Month, Amount) VALUES (@CategoryId, @Month, @Amount)
                ON CONFLICT(CategoryId, Month) DO UPDATE SET Amount = @Amount", budget);
        }
        finally { ReleaseConnection(conn); }
    }

    public void DeleteBudget(int id)
    {
        var conn = GetConnection();
        try { conn.Execute("DELETE FROM Budgets WHERE Id=@id", new { id }); }
        finally { ReleaseConnection(conn); }
    }
}
