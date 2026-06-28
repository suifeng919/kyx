namespace MoneyTracker.Core.Models;

public class MonthlySummary
{
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}
