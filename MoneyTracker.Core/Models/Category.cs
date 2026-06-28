namespace MoneyTracker.Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Type { get; set; }       // 0=支出 1=收入
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}
