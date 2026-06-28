using MoneyTracker.Core.Interfaces;
using MoneyTracker.Data;
using MoneyTracker.WinForm.Controls;

namespace MoneyTracker.WinForm.Forms;

public partial class MainForm : Form
{
    public IDataService DataService { get; }
    public int CurrentYear { get; private set; }
    public int CurrentMonth { get; private set; }

    private readonly TabControl _tabControl;
    private readonly TabPage _tabBill;
    private readonly TabPage _tabStats;
    private readonly TabPage _tabBudget;
    private BillListControl _billListControl = null!;
    private StatsControl _statsControl = null!;
    private BudgetControl _budgetControl = null!;
    private readonly Label _lblMonth;

    public MainForm()
    {
        // 初始化数据服务
        DataService = new DataService(Program.ConnectionString);

        var now = DateTime.Now;
        CurrentYear = now.Year;
        CurrentMonth = now.Month;

        // 窗体设置
        Text = "MoneyTracker 个人记账本";
        Size = new Size(900, 650);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("微软雅黑", 10);

        // 顶部布局
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(64, 158, 255)
        };

        var lblTitle = new Label
        {
            Text = "📒 MoneyTracker",
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 16, FontStyle.Bold),
            Location = new Point(15, 10),
            AutoSize = true
        };

        // 右侧导航栏用一个独立面板 Dock = Right，彻底避免定位问题
        var navPanel = new Panel
        {
            Width = 200,
            Height = 50,
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(64, 158, 255)
        };

        var btnPrevMonth = new Button
        {
            Text = "◀",
            Location = new Point(10, 12),
            Size = new Size(35, 28),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(64, 158, 255),
            FlatAppearance = { BorderSize = 0 }
        };
        btnPrevMonth.Click += (_, _) => ChangeMonth(-1);

        _lblMonth = new Label
        {
            Text = $"{CurrentYear}年{CurrentMonth}月",
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 12, FontStyle.Bold),
            Location = new Point(50, 13),
            AutoSize = true
        };

        var btnNextMonth = new Button
        {
            Text = "▶",
            Location = new Point(155, 12),
            Size = new Size(35, 28),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(64, 158, 255),
            FlatAppearance = { BorderSize = 0 }
        };
        btnNextMonth.Click += (_, _) => ChangeMonth(1);

        navPanel.Controls.AddRange([btnPrevMonth, _lblMonth, btnNextMonth]);
        topPanel.Controls.AddRange([lblTitle, navPanel]);

        // TabControl（底部导航风格）
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("微软雅黑", 12),
            Alignment = TabAlignment.Bottom,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(300, 45),
            Padding = new Point(20, 8)
        };

        // Tab: 账单
        _tabBill = new TabPage("📋 账单");
        _billListControl = new BillListControl(this)
        {
            Dock = DockStyle.Fill
        };
        _tabBill.Controls.Add(_billListControl);

        // Tab: 统计
        _tabStats = new TabPage("📊 统计");
        _statsControl = new StatsControl(this)
        {
            Dock = DockStyle.Fill
        };
        _tabStats.Controls.Add(_statsControl);

        // Tab: 预算
        _tabBudget = new TabPage("💰 预算");
        _budgetControl = new BudgetControl(this)
        {
            Dock = DockStyle.Fill
        };
        _tabBudget.Controls.Add(_budgetControl);

        _tabControl.TabPages.AddRange([_tabBill, _tabStats, _tabBudget]);
        _tabControl.SelectedIndexChanged += (_, _) => RefreshCurrentTab();

        Controls.AddRange([_tabControl, topPanel]);
    }

    private void ChangeMonth(int delta)
    {
        var date = new DateTime(CurrentYear, CurrentMonth, 1).AddMonths(delta);
        CurrentYear = date.Year;
        CurrentMonth = date.Month;
        UpdateMonthDisplay();
        RefreshCurrentTab();
    }

    private void UpdateMonthDisplay()
    {
        _lblMonth.Text = $"{CurrentYear}年{CurrentMonth}月";
    }

    private void RefreshCurrentTab()
    {
        if (_tabControl.SelectedTab == _tabBill)
            _billListControl.RefreshData();
        else if (_tabControl.SelectedTab == _tabStats)
            _statsControl.RefreshData();
        else if (_tabControl.SelectedTab == _tabBudget)
            _budgetControl.RefreshData();
    }

    private void InitializeComponent()
    {

    }

    public void RefreshAll()
    {
        _billListControl.RefreshData();
        _statsControl.RefreshData();
        _budgetControl.RefreshData();
    }
}
