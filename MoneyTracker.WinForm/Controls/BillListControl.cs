using MoneyTracker.Core.Models;
using MoneyTracker.WinForm.Forms;

namespace MoneyTracker.WinForm.Controls;

public class BillListControl : UserControl
{
    private readonly MainForm _mainForm;
    private ListView _lvBills = null!;
    private ComboBox _cmbCategoryFilter = null!;
    private Label _lblSummary = null!;
    private Button _btnAdd = null!;
    private Button _btnExport = null!;

    public BillListControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeControl();
    }

    private void InitializeControl()
    {
        // 使用 FlowLayoutPanel 让工具栏自适应
        var toolPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(10, 8, 10, 0),
            AutoSize = false
        };

        _btnAdd = new Button
        {
            Text = "＋ 新增账单",
            Size = new Size(110, 32),
            BackColor = Color.FromArgb(64, 158, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Margin = new Padding(0, 0, 15, 0)
        };
        _btnAdd.Click += (_, _) =>
        {
            using var form = new AddBillForm(_mainForm);
            form.ShowDialog(this);
        };

        var lblFilter = new Label
        {
            Text = "分类筛选：",
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 5, 5, 0)
        };
        _cmbCategoryFilter = new ComboBox
        {
            Size = new Size(130, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 2, 15, 0)
        };
        _cmbCategoryFilter.Items.Add("全部");
        _cmbCategoryFilter.SelectedIndex = 0;
        _cmbCategoryFilter.SelectedIndexChanged += (_, _) => RefreshData();

        _btnExport = new Button
        {
            Text = "📥 导出Excel",
            Size = new Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 15, 0)
        };
        _btnExport.Click += BtnExport_Click;

        _lblSummary = new Label
        {
            Text = "本月支出: ¥0.00",
            AutoSize = true,
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 158, 255),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 5, 0, 0)
        };

        toolPanel.Controls.AddRange([_btnAdd, lblFilter, _cmbCategoryFilter, _btnExport, _lblSummary]);

        // 账单列表
        _lvBills = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("微软雅黑", 10),
            MultiSelect = false
        };
        _lvBills.Columns.Add("日期", 100);
        _lvBills.Columns.Add("分类", 90);
        _lvBills.Columns.Add("类型", 55);
        _lvBills.Columns.Add("金额", 90, HorizontalAlignment.Right);
        _lvBills.Columns.Add("备注", 200);

        // 右键菜单
        var ctxMenu = new ContextMenuStrip();
        var miEdit = new ToolStripMenuItem("编辑");
        miEdit.Click += (_, _) => EditSelectedBill();
        var miDelete = new ToolStripMenuItem("删除");
        miDelete.Click += (_, _) => DeleteSelectedBill();
        ctxMenu.Items.AddRange([miEdit, miDelete]);
        _lvBills.ContextMenuStrip = ctxMenu;

        // 双击编辑
        _lvBills.DoubleClick += (_, _) => EditSelectedBill();

        // 加载分类列表到筛选器
        var categories = _mainForm.DataService.GetCategories();
        foreach (var c in categories.Where(c => c.Type == 0))
            _cmbCategoryFilter.Items.Add(c.Name);

        Controls.AddRange([_lvBills, toolPanel]);
    }

    public void RefreshData()
    {
        _lvBills.Items.Clear();

        int? catId = null;
        if (_cmbCategoryFilter.SelectedIndex > 0)
        {
            var categories = _mainForm.DataService.GetCategories();
            var selected = categories
                .Where(c => c.Type == 0)
                .ElementAtOrDefault(_cmbCategoryFilter.SelectedIndex - 1);
            if (selected != null) catId = selected.Id;
        }

        var bills = _mainForm.DataService.GetBills(
            _mainForm.CurrentYear, _mainForm.CurrentMonth, catId);

        decimal totalExpense = 0;
        foreach (var b in bills)
        {
            var item = new ListViewItem(b.Date);
            item.SubItems.Add(b.CategoryName ?? "");
            item.SubItems.Add(b.IsIncome ? "收入" : "支出");
            item.SubItems.Add(b.Amount.ToString("F2"));
            item.SubItems.Add(b.Remark ?? "");
            item.Tag = b.Id;

            if (!b.IsIncome) totalExpense += b.Amount;

            _lvBills.Items.Add(item);
        }

        _lblSummary.Text = $"本月支出: ¥{totalExpense:F2}  |  共 {bills.Count} 笔";
    }

    private void EditSelectedBill()
    {
        if (_lvBills.SelectedItems.Count == 0) return;
        int id = (int)_lvBills.SelectedItems[0].Tag!;
        var bill = _mainForm.DataService.GetBillById(id);
        if (bill == null) return;

        using var form = new AddBillForm(_mainForm, bill);
        form.ShowDialog(this);
    }

    private void DeleteSelectedBill()
    {
        if (_lvBills.SelectedItems.Count == 0) return;
        int id = (int)_lvBills.SelectedItems[0].Tag!;

        if (MessageBox.Show("确定要删除这条账单吗？", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _mainForm.DataService.DeleteBill(id);
            RefreshData();
            if (Parent?.Parent is MainForm mf) mf.RefreshAll();
        }
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Excel文件|*.xlsx",
            FileName = $"账单_{_mainForm.CurrentYear}_{_mainForm.CurrentMonth:D2}.xlsx"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var exportService = new Core.Services.ExportService(_mainForm.DataService);
                exportService.ExportToExcel(_mainForm.CurrentYear, _mainForm.CurrentMonth, dlg.FileName);
                MessageBox.Show("导出成功！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
