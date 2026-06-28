using MoneyTracker.Data;
using MoneyTracker.WinForm.Forms;

namespace MoneyTracker.WinForm;

internal static class Program
{
    // 数据库连接字符串（数据库文件放在应用程序同级目录）
    public static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "MoneyTracker.db");
    public static readonly string ConnectionString = $"Data Source={DbPath}";

    [STAThread]
    static void Main()
    {
        // 初始化数据库（建表+预设分类）
        var initializer = new DatabaseInitializer(ConnectionString);
        initializer.Initialize();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
