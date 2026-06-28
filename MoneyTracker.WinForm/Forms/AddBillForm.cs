using MoneyTracker.Core.Models;

namespace MoneyTracker.WinForm.Forms;

public partial class AddBillForm : Form
{
    private readonly MainForm _mainForm;
    private readonly Bill? _editBill;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtAmount = null!;
    private DateTimePicker _dtpDate = null!;
    private TextBox _txtRemark = null!;
    private CheckBox _chkIncome = null!;

    /// <summary>编辑模式构造函数</summary>
    public AddBillForm(MainForm mainForm, Bill billToEdit) : this(mainForm)
    {
        _editBill = billToEdit;
        Text = "编辑账单";
        _txtAmount.Text = billToEdit.Amount.ToString("F2");
        _dtpDate.Value = DateTime.Parse(billToEdit.Date);
        _txtRemark.Text = billToEdit.Remark ?? "";
        _chkIncome.Checked = billToEdit.IsIncome;
        _cmbCategory.SelectedValue = billToEdit.CategoryId;
    }

    /// <summary>新增模式构造函数</summary>
    public AddBillForm(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Text = "新增账单";
        Size = new Size(440, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("微软雅黑", 10);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            BackColor = Color.White
        };

        int y = 15;

        // 金额
        panel.Controls.Add(CreateLabel("金额 (元)：", 10, y));
        _txtAmount = new TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(240, 32),
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Right
        };
        panel.Controls.Add(_txtAmount);
        y += 45;

        // 分类
        panel.Controls.Add(CreateLabel("分类：", 10, y));
        _cmbCategory = new ComboBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(240, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Name",
            ValueMember = "Id"
        };
        var categories = _mainForm.DataService.GetCategories();
        _cmbCategory.DataSource = categories.Where(c => c.Type == 0).ToList();
        panel.Controls.Add(_cmbCategory);
        y += 45;

        // 日期
        panel.Controls.Add(CreateLabel("日期：", 10, y));
        _dtpDate = new DateTimePicker
        {
            Location = new Point(120, y - 3),
            Size = new Size(240, 30),
            Value = DateTime.Today,
            Format = DateTimePickerFormat.Short
        };
        panel.Controls.Add(_dtpDate);
        y += 45;

        // 备注
        panel.Controls.Add(CreateLabel("备注：", 10, y));
        _txtRemark = new TextBox
        {
            Location = new Point(120, y - 3),
            Size = new Size(200, 30)
        };
        panel.Controls.Add(_txtRemark);
        y += 45;

        // 收入复选框
        _chkIncome = new CheckBox
        {
            Text = "这是收入",
            Location = new Point(120, y),
            AutoSize = true
        };
        _chkIncome.CheckedChanged += (_, _) =>
        {
            _cmbCategory.DataSource = _chkIncome.Checked
                ? _mainForm.DataService.GetCategories().Where(c => c.Type == 1).ToList()
                : _mainForm.DataService.GetCategories().Where(c => c.Type == 0).ToList();
        };
        panel.Controls.Add(_chkIncome);
        y += 40;

        // 按钮
        var btnSave = new Button
        {
            Text = "保存",
            Location = new Point(120, y),
            Size = new Size(90, 35),
            BackColor = Color.FromArgb(64, 158, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 }
        };
        btnSave.Click += BtnSave_Click;

        var btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(220, y),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (_, _) => Close();

        panel.Controls.AddRange([btnSave, btnCancel]);
        Controls.Add(panel);
    }

    private static Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // 验证
        if (!decimal.TryParse(_txtAmount.Text, out decimal amount) || amount <= 0)
        {
            MessageBox.Show("请输入有效的金额！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbCategory.SelectedValue == null)
        {
            MessageBox.Show("请选择分类！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_editBill != null)
        {
            // 编辑模式
            _editBill.CategoryId = (int)_cmbCategory.SelectedValue;
            _editBill.Amount = amount;
            _editBill.IsIncome = _chkIncome.Checked;
            _editBill.Date = _dtpDate.Value.ToString("yyyy-MM-dd");
            _editBill.Remark = _txtRemark.Text.Trim();
            _mainForm.DataService.UpdateBill(_editBill);
        }
        else
        {
            // 新增模式
            var bill = new Bill
            {
                CategoryId = (int)_cmbCategory.SelectedValue,
                Amount = amount,
                IsIncome = _chkIncome.Checked,
                Date = _dtpDate.Value.ToString("yyyy-MM-dd"),
                Remark = _txtRemark.Text.Trim(),
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            _mainForm.DataService.AddBill(bill);
        }

        _mainForm.RefreshAll();
        Close();
    }
}
