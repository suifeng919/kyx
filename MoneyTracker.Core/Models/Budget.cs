namespace MoneyTracker.Core.Models;

public class Budget
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Month { get; set; } = DateTime.Now.ToString("yyyy-MM");
    public decimal Amount { get; set; }

    public string? CategoryName { get; set; }
}
