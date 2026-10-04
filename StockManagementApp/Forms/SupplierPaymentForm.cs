using StockManagementApp.BusinessLogic;
using System.Data;

namespace StockManagementApp.Forms;

public class SupplierPaymentForm : Form
{
    private readonly SupplierPaymentService _service = new();
    private readonly SupplierService _supplierService = new();
    private DataGridView dgvPayments = null!;
    private ComboBox cmbSupplier = null!;
    private TextBox txtAmount = null!;
    private ComboBox cmbMethod = null!;
    private TextBox txtReference = null!;
    private Button btnSave = null!;

    public SupplierPaymentForm()
    {
        InitializeComponent();
        LoadSuppliers();
        LoadPayments();
    }

    private void InitializeComponent()
    {
        Text = "Supplier Payments";
        Size = new Size(860, 580);
        MinimumSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 246, 250);

        Controls.Add(new Label
        {
            Text = "Supplier Payments",
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

        entryPanel.Controls.Add(new Label { Text = "Supplier:", Location = new Point(10, 17), Width = 60, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        cmbSupplier = new ComboBox { Location = new Point(74, 14), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        entryPanel.Controls.Add(cmbSupplier);

        entryPanel.Controls.Add(new Label { Text = "Amount:", Location = new Point(286, 17), Width = 58, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtAmount = new TextBox { Location = new Point(348, 14), Width = 100 };
        entryPanel.Controls.Add(txtAmount);

        entryPanel.Controls.Add(new Label { Text = "Method:", Location = new Point(460, 17), Width = 56, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        cmbMethod = new ComboBox { Location = new Point(520, 14), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbMethod.Items.AddRange(new object[] { "Cash", "Bank", "UPI", "Cheque", "Other" });
        cmbMethod.SelectedIndex = 0;
        entryPanel.Controls.Add(cmbMethod);

        entryPanel.Controls.Add(new Label { Text = "Reference:", Location = new Point(644, 17), Width = 68, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtReference = new TextBox { Location = new Point(716, 14), Width = 75 };
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

    private void LoadSuppliers()
    {
        var suppliers = _supplierService.GetSuppliers();
        cmbSupplier.DisplayMember = "SupplierName";
        cmbSupplier.ValueMember = "SupplierId";
        cmbSupplier.DataSource = suppliers;
    }

    private void LoadPayments()
    {
        dgvPayments.DataSource = _service.GetPayments();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (cmbSupplier.SelectedValue == null)
        {
            MessageBox.Show("Please select a supplier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            _service.AddPayment(Convert.ToInt32(cmbSupplier.SelectedValue), amount, method, reference);
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
