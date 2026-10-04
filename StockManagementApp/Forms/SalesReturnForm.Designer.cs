namespace StockManagementApp.Forms;

partial class SalesReturnForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSaleId;
    private System.Windows.Forms.TextBox txtSaleId;
    private System.Windows.Forms.Label lblQuantity;
    private System.Windows.Forms.TextBox txtQuantity;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.DataGridView dgvReturns;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Sales Return";
        this.Size = new System.Drawing.Size(860, 560);
        this.MinimumSize = new Size(760, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        // ── Page title ─────────────────────────────────────────────────────────
        lblTitle = new Label
        {
            Text = "Sales Return",
            Location = new Point(20, 12),
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        // ── Entry panel ────────────────────────────────────────────────────────
        var entryPanel = new Panel
        {
            Location = new Point(20, 46),
            Size = new Size(800, 70),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        entryPanel.Controls.Add(new Label { Text = "Sale ID:", Location = new Point(10, 20), Width = 52, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtSaleId = new TextBox { Location = new Point(66, 17), Width = 100 };
        entryPanel.Controls.Add(txtSaleId);
        lblSaleId = new Label { Text = "Sale ID:", Location = new Point(10, 20), Width = 52 };

        entryPanel.Controls.Add(new Label { Text = "Return Qty:", Location = new Point(180, 20), Width = 74, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 65, 81) });
        txtQuantity = new TextBox { Location = new Point(258, 17), Width = 100 };
        entryPanel.Controls.Add(txtQuantity);
        lblQuantity = new Label { Text = "Return Qty:", Location = new Point(180, 20), Width = 74 };

        btnSave = new Button
        {
            Text = "Process Return",
            Location = new Point(376, 15),
            Size = new Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(249, 115, 22),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += new EventHandler(btnSave_Click);
        entryPanel.Controls.Add(btnSave);

        entryPanel.Controls.Add(new Label
        {
            Text = "Tip: Find the Sale ID from the Sales module history grid.",
            Location = new Point(510, 22),
            AutoSize = true,
            ForeColor = Color.FromArgb(107, 114, 128),
            Font = new Font("Segoe UI", 8)
        });

        Controls.Add(entryPanel);

        // ── Section label ──────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text = "Return History",
            Location = new Point(20, 130),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        // ── Grid ───────────────────────────────────────────────────────────────
        dgvReturns = new DataGridView
        {
            Location = new Point(20, 150),
            Size = new Size(800, 360),
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
        Controls.Add(dgvReturns);
    }
}
