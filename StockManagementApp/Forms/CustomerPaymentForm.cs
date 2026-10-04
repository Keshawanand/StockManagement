using StockManagementApp.BusinessLogic;
using System.Data;

namespace StockManagementApp.Forms;

public class CustomerPaymentForm : Form
{
    private readonly CustomerPaymentService _service = new();
    private readonly CustomerService _customerService = new();
    private DataGridView dgvPayments = null!;
    private ComboBox cmbCustomer = null!;
    private TextBox txtAmount = null!;
    private ComboBox cmbMethod = null!;
    private TextBox txtReference = null!;
    private Button btnSave = null!;

    public CustomerPaymentForm()
    {
        InitializeComponent();
        LoadCustomers();
        LoadPayments();
    }

    private void InitializeComponent()
    {
        Text = "Customer Payments";
        Size = new Size(860, 580);
        MinimumSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 246, 250);

        Controls.Add(new Label
        {
            Text = "Customer Payments",
            Location = new Point(20, 12),
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        });

        // ── Entry panel ────────────────────────────────────────────────────────
        var entryPanel = new Panel
        {
            Location = new Point(20, 46),
            Size = new Size(800, 80),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        entryPanel.Controls.Add(new Label { Text = "Customer:", Location = new Point(10, 17), Width = 68, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        cmbCustomer = new ComboBox { Location = new Point(82, 14), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        entryPanel.Controls.Add(cmbCustomer);

        entryPanel.Controls.Add(new Label { Text = "Amount:", Location = new Point(294, 17), Width = 58, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtAmount = new TextBox { Location = new Point(356, 14), Width = 100 };
        entryPanel.Controls.Add(txtAmount);

        entryPanel.Controls.Add(new Label { Text = "Method:", Location = new Point(470, 17), Width = 56, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        cmbMethod = new ComboBox { Location = new Point(530, 14), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbMethod.Items.AddRange(new object[] { "Cash", "Bank", "UPI", "Cheque", "Other" });
        cmbMethod.SelectedIndex = 0;
        entryPanel.Controls.Add(cmbMethod);

        entryPanel.Controls.Add(new Label { Text = "Reference:", Location = new Point(654, 17), Width = 68, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtReference = new TextBox { Location = new Point(726, 14), Width = 65 };
        entryPanel.Controls.Add(txtReference);

        btnSave = new Button
        {
            Text = "Save Payment",
            Location = new Point(608, 46),
            Size = new Size(116, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;
        entryPanel.Controls.Add(btnSave);
        Controls.Add(entryPanel);

        // ── Section label ──────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text = "Payment History",
            Location = new Point(20, 140),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        // ── Grid ───────────────────────────────────────────────────────────────
        dgvPayments = new DataGridView
        {
            Location = new Point(20, 160),
            Size = new Size(800, 370),
            ReadOnly = true,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(220, 220, 235),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 30,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 26 },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 80),
                Padding = new Padding(4, 0, 4, 0)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.Black
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 255)
            }
        };
        Controls.Add(dgvPayments);
    }

    private void LoadCustomers()
    {
        var customers = _customerService.GetCustomers();
        cmbCustomer.DisplayMember = "CustomerName";
        cmbCustomer.ValueMember = "CustomerId";
        cmbCustomer.DataSource = customers;
    }

    private void LoadPayments()
    {
        dgvPayments.DataSource = _service.GetPayments();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (cmbCustomer.SelectedValue == null)
        {
            MessageBox.Show("Please select a customer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtAmount.Text.Trim(), out var amount) || amount <= 0)
        {
            MessageBox.Show("Please enter a valid amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var method = cmbMethod.SelectedItem?.ToString() ?? "Cash";
        var reference = txtReference.Text.Trim();

        try
        {
            _service.AddPayment(Convert.ToInt32(cmbCustomer.SelectedValue), amount, method, reference);
            LoadPayments();
            txtAmount.Text = string.Empty;
            txtReference.Text = string.Empty;
            MessageBox.Show("Payment recorded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
