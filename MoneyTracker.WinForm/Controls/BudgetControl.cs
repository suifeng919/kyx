using MoneyTracker.Core.Models;
using MoneyTracker.Core.Services;
using MoneyTracker.WinForm.Forms;

namespace MoneyTracker.WinForm.Controls;

public class BudgetControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly BudgetService _budgetService;
    private ListView _lvBudgets = null!;
    private Label _lblStatus = null!;

    public BudgetControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        _budgetService = new BudgetService(mainForm.DataService);
        InitializeControl();
    }

    private void InitializeControl()
    {
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(10, 5, 10, 0)
        };

        _lblStatus = new Label
        {
            Text = "点击下方「设置预算」按钮为各分类设定月度预算",
            Location = new Point(10, 8),
            AutoSize = true,
            ForeColor = Color.Gray
        };

        var btnSetBudget = new Button
        {
            Text = "设置预算",
            Location = new Point(350, 5),
            Size = new Size(100, 28),
            BackColor = Color.FromArgb(64, 158, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 }
        };
        btnSetBudget.Click += BtnSetBudget_Click;

        topPanel.Controls.AddRange([_lblStatus, btnSetBudget]);

        // 预算列表
        _lvBudgets = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("微软雅黑", 10)
        };
        _lvBudgets.Columns.Add("分类", 120);
        _lvBudgets.Columns.Add("预算金额", 120, HorizontalAlignment.Right);
        _lvBudgets.Columns.Add("已支出", 120, HorizontalAlignment.Right);
        _lvBudgets.Columns.Add("剩余", 120, HorizontalAlignment.Right);
        _lvBudgets.Columns.Add("状态", 100);

        Controls.AddRange([_lvBudgets, topPanel]);
    }

    public void RefreshData()
    {
        _lvBudgets.Items.Clear();

        string month = $"{_mainForm.CurrentYear}-{_mainForm.CurrentMonth:D2}";
        var statusList = _budgetService.GetBudgetStatusList(month);

        int overCount = 0;
        foreach (var (budget, spent, isOver) in statusList)
        {
            var item = new ListViewItem(budget.CategoryName);
            item.SubItems.Add($"¥{budget.Amount:F2}");
            item.SubItems.Add($"¥{spent:F2}");
            item.SubItems.Add($"¥{Math.Max(0, budget.Amount - spent):F2}");
            item.SubItems.Add(isOver ? "⚠ 超支" : "✅ 正常");
            item.BackColor = isOver ? Color.LightPink : Color.White;

            if (isOver) overCount++;
            _lvBudgets.Items.Add(item);
        }

        if (statusList.Count == 0)
        {
            _lblStatus.Text = "本月尚未设置预算，点击上方按钮设置";
            _lblStatus.ForeColor = Color.Gray;
        }
        else if (overCount > 0)
        {
            _lblStatus.Text = $"⚠ 有 {overCount} 个分类已超支！";
            _lblStatus.ForeColor = Color.Red;
        }
        else
        {
            _lblStatus.Text = "✅ 所有分类均在预算范围内";
            _lblStatus.ForeColor = Color.Green;
        }
    }

    private void BtnSetBudget_Click(object? sender, EventArgs e)
    {
        // 弹出预算设置对话框
        string month = $"{_mainForm.CurrentYear}-{_mainForm.CurrentMonth:D2}";
        var categories = _mainForm.DataService.GetCategories()
            .Where(c => c.Type == 0).ToList();
        var existing = _mainForm.DataService.GetBudgets(month);

        using var form = new Form
        {
            Text = "设置预算",
            Size = new Size(400, 450),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            Font = new Font("微软雅黑", 10)
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        var controls = new Dictionary<int, NumericUpDown>();

        foreach (var cat in categories)
        {
            var row = new Panel { Height = 35, Width = 340 };
            var lbl = new Label
            {
                Text = cat.Name,
                Location = new Point(0, 8),
                AutoSize = true,
                Width = 100
            };
            var nud = new NumericUpDown
            {
                Location = new Point(110, 5),
                Width = 150,
                Maximum = 99999,
                DecimalPlaces = 2,
                Increment = 10
            };

            var existingBudget = existing.FirstOrDefault(b => b.CategoryId == cat.Id);
            if (existingBudget != null)
                nud.Value = existingBudget.Amount;

            controls[cat.Id] = nud;
            row.Controls.AddRange([lbl, nud]);
            panel.Controls.Add(row);
        }

        var btnSave = new Button
        {
            Text = "保存预算",
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(64, 158, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Margin = new Padding(0, 10, 0, 0)
        };
        btnSave.Click += (_, _) =>
        {
            foreach (var kvp in controls)
            {
                var budget = new Budget
                {
                    CategoryId = kvp.Key,
                    Month = month,
                    Amount = kvp.Value.Value
                };
                _mainForm.DataService.SetBudget(budget);
            }
            RefreshData();
            form.Close();
        };

        panel.Controls.Add(btnSave);
        form.Controls.Add(panel);
        form.ShowDialog(this);
    }
}
