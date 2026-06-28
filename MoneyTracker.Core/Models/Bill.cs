namespace MoneyTracker.Core.Models;

public class Bill
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public string? Remark { get; set; }
    public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    // 导航属性（仅用于展示，不存数据库）
    public string? CategoryName { get; set; }
}
