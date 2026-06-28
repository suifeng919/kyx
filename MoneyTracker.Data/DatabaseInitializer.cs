using Dapper;
using System.Data.SQLite;

namespace MoneyTracker.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Initialize()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();

        // 开启WAL模式提升并发性能
        conn.Execute("PRAGMA journal_mode=WAL;");

        CreateTables(conn);
        SeedDefaultCategories(conn);
    }

    private void CreateTables(SQLiteConnection conn)
    {
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS Categories (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT NOT NULL,
                Icon        TEXT,
                Type        INTEGER NOT NULL DEFAULT 0,
                SortOrder   INTEGER DEFAULT 0,
                IsDefault   INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Bills (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId  INTEGER NOT NULL,
                Amount      DECIMAL(10,2) NOT NULL,
                IsIncome    INTEGER NOT NULL DEFAULT 0,
                Date        TEXT NOT NULL,
                Remark      TEXT,
                CreatedAt   TEXT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );

            CREATE TABLE IF NOT EXISTS Budgets (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId  INTEGER NOT NULL,
                Month       TEXT NOT NULL,
                Amount      DECIMAL(10,2) NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                UNIQUE(CategoryId, Month)
            );
        ");
    }

    private void SeedDefaultCategories(SQLiteConnection conn)
    {
        int count = conn.QuerySingle<int>("SELECT COUNT(*) FROM Categories");
        if (count > 0) return;

        var categories = new[]
        {
            new { Name = "食堂",      Icon = "fa-utensils",    Type = 0, SortOrder = 1  },
            new { Name = "外卖",      Icon = "fa-shopping-bag",Type = 0, SortOrder = 2  },
            new { Name = "奶茶零食",  Icon = "fa-candy-cane",  Type = 0, SortOrder = 3  },
            new { Name = "日用品",    Icon = "fa-box",          Type = 0, SortOrder = 4  },
            new { Name = "服饰",      Icon = "fa-tshirt",       Type = 0, SortOrder = 5  },
            new { Name = "交通",      Icon = "fa-bus",          Type = 0, SortOrder = 6  },
            new { Name = "学习用品",  Icon = "fa-book",         Type = 0, SortOrder = 7  },
            new { Name = "娱乐",      Icon = "fa-gamepad",      Type = 0, SortOrder = 8  },
            new { Name = "通讯话费",  Icon = "fa-phone",        Type = 0, SortOrder = 9  },
            new { Name = "其他支出",  Icon = "fa-ellipsis-h",   Type = 0, SortOrder = 10 },
            new { Name = "兼职工资",  Icon = "fa-briefcase",    Type = 1, SortOrder = 11 },
            new { Name = "其他收入",  Icon = "fa-coins",        Type = 1, SortOrder = 12 },
        };

        conn.Execute(@"
            INSERT INTO Categories (Name, Icon, Type, SortOrder, IsDefault)
            VALUES (@Name, @Icon, @Type, @SortOrder, 1)", categories);
    }
}
