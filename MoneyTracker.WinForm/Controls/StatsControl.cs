using System.Windows.Forms.DataVisualization.Charting;
using MoneyTracker.Core.Services;
using MoneyTracker.WinForm.Forms;

namespace MoneyTracker.WinForm.Controls;

public class StatsControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly BillService _billService;
    private TabControl _chartTab = null!;
    private Chart _pieChart = null!;
    private Chart _barChart = null!;
    private Label _lblIncome = null!;
    private Label _lblExpense = null!;
    private Label _lblBalance = null!;

    public StatsControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        _billService = new BillService(mainForm.DataService);
        InitializeControl();
    }

    private void InitializeControl()
    {
        // 顶部摘要
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(15, 8, 15, 0)
        };

        _lblIncome = new Label { AutoSize = true, Location = new Point(15, 10),
            Font = new Font("微软雅黑", 11) };
        _lblExpense = new Label { AutoSize = true, Location = new Point(200, 10),
            Font = new Font("微软雅黑", 11) };
        _lblBalance = new Label { AutoSize = true, Location = new Point(400, 10),
            Font = new Font("微软雅黑", 11, FontStyle.Bold) };

        topPanel.Controls.AddRange([_lblIncome, _lblExpense, _lblBalance]);

        // 图表Tab切换
        _chartTab = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("微软雅黑", 10)
        };

        // Tab1: 支出分布（饼图）
        var tabPie = new TabPage("支出分类分布");
        _pieChart = CreateChart(SeriesChartType.Pie, "支出分布");
        tabPie.Controls.Add(_pieChart);

        // Tab2: 收支对比（柱状图）
        var tabBar = new TabPage("月度收支对比");
        _barChart = CreateChart(SeriesChartType.Column, "收支对比");
        tabBar.Controls.Add(_barChart);

        _chartTab.TabPages.AddRange([tabPie, tabBar]);

        Controls.AddRange([_chartTab, topPanel]);
    }

    private Chart CreateChart(SeriesChartType chartType, string chartName)
    {
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        var area = new ChartArea("MainArea");
        area.AxisX.LabelStyle.Font = new Font("微软雅黑", 9);
        area.AxisY.LabelStyle.Font = new Font("微软雅黑", 9);
        area.Area3DStyle.Enable3D = chartType == SeriesChartType.Pie;

        chart.ChartAreas.Add(area);

        var series = new Series(chartName)
        {
            ChartType = chartType,
            IsValueShownAsLabel = true,
            LabelForeColor = Color.Black,
            Font = new Font("微软雅黑", 9)
        };

        if (chartType == SeriesChartType.Pie)
        {
            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"] = "Black";
        }

        chart.Series.Add(series);
        return chart;
    }

    public void RefreshData()
    {
        int year = _mainForm.CurrentYear;
        int month = _mainForm.CurrentMonth;

        // 摘要数据
        decimal income = _mainForm.DataService.GetMonthlyTotal(year, month, true);
        decimal expense = _mainForm.DataService.GetMonthlyTotal(year, month, false);
        decimal balance = income - expense;

        _lblIncome.Text = $"💰 收入: ¥{income:F2}";
        _lblIncome.ForeColor = Color.Green;
        _lblExpense.Text = $"💸 支出: ¥{expense:F2}";
        _lblExpense.ForeColor = Color.Red;
        _lblBalance.Text = balance >= 0
            ? $"📈 结余: ¥{balance:F2}"
            : $"📉 超支: ¥{Math.Abs(balance):F2}";
        _lblBalance.ForeColor = balance >= 0 ? Color.FromArgb(64, 158, 255) : Color.Red;

        // 饼图：支出分类分布
        var pieSeries = _pieChart.Series[0];
        pieSeries.Points.Clear();

        var summary = _billService.GetSummaryWithPercentages(year, month, isIncome: false);
        foreach (var item in summary)
        {
            var point = pieSeries.Points.Add((double)item.TotalAmount);
            point.LegendText = $"{item.CategoryName} ({item.Percentage}%)";
            point.Label = $"¥{item.TotalAmount:F0}";
            point.Color = GetCategoryColor(item.CategoryId);
        }

        // 柱状图：各分类支出对比
        var barSeries = _barChart.Series[0];
        barSeries.Points.Clear();

        foreach (var item in summary)
        {
            var point = barSeries.Points.Add((double)item.TotalAmount);
            point.AxisLabel = item.CategoryName;
            point.Label = $"¥{item.TotalAmount:F0}";
            point.Color = GetCategoryColor(item.CategoryId);
        }

        // 如果有收入数据，添加到柱状图
        var incomeSummary = _billService.GetSummaryWithPercentages(year, month, isIncome: true);
        if (incomeSummary.Count > 0 && barSeries.Points.Count > 0)
        {
            // 在柱状图中添加一条收入合计线
            decimal totalIncome = incomeSummary.Sum(i => i.TotalAmount);
            var incomePoint = barSeries.Points.Add((double)totalIncome);
            incomePoint.AxisLabel = "收入";
            incomePoint.Label = $"¥{totalIncome:F0}";
            incomePoint.Color = Color.Green;
        }
    }

    private static Color GetCategoryColor(int categoryId)
    {
        // 给不同分类分配不同的颜色
        var colors = new[]
        {
            Color.FromArgb(64, 158, 255),  // 蓝
            Color.FromArgb(255, 193, 7),   // 黄
            Color.FromArgb(103, 194, 58),  // 绿
            Color.FromArgb(245, 108, 108), // 红
            Color.FromArgb(144, 147, 153), // 灰
            Color.FromArgb(230, 162, 60),  // 橙
            Color.FromArgb(121, 134, 203), // 紫
            Color.FromArgb(77, 189, 183),  // 青
            Color.FromArgb(198, 120, 221), // 粉紫
            Color.FromArgb(255, 154, 108), // 杏色
        };
        return colors[categoryId % colors.Length];
    }
}
