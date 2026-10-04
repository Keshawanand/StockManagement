namespace StockManagementApp.Forms;

partial class SupplierForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSupplierName;
    private System.Windows.Forms.TextBox txtSupplierName;
    private System.Windows.Forms.Label lblMobile;
    private System.Windows.Forms.TextBox txtMobile;
    private System.Windows.Forms.Label lblEmail;
    private System.Windows.Forms.TextBox txtEmail;
    private System.Windows.Forms.Label lblAddress;
    private System.Windows.Forms.TextBox txtAddress;
    private System.Windows.Forms.Label lblGst;
    private System.Windows.Forms.TextBox txtGst;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.DataGridView dgvSuppliers;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Supplier Management";
        this.Size = new System.Drawing.Size(900, 540);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        lblTitle = new Label
        {
            Text      = "Supplier Management",
            Location  = new Point(20, 14),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        Controls.Add(new Label
        {
            Text      = "Supplier Details",
            Location  = new Point(20, 50),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        // ── Entry panel ───────────────────────────────────────────────────────
        var entryPanel = new Panel
        {
            Location    = new Point(20, 68),
            Size        = new Size(852, 130),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Row 1: Name | Mobile
        entryPanel.Controls.Add(new Label { Text = "Name",   Location = new Point(10, 15), AutoSize = true });
        txtSupplierName = new TextBox { Location = new Point(70, 11), Width = 250 };
        entryPanel.Controls.Add(txtSupplierName);

        entryPanel.Controls.Add(new Label { Text = "Mobile", Location = new Point(340, 15), AutoSize = true });
        txtMobile = new TextBox { Location = new Point(400, 11), Width = 180 };
        entryPanel.Controls.Add(txtMobile);

        // Row 2: Email | Address
        entryPanel.Controls.Add(new Label { Text = "Email",   Location = new Point(10, 55), AutoSize = true });
        txtEmail = new TextBox { Location = new Point(70, 51), Width = 250 };
        entryPanel.Controls.Add(txtEmail);

        entryPanel.Controls.Add(new Label { Text = "Address", Location = new Point(340, 55), AutoSize = true });
        txtAddress = new TextBox { Location = new Point(400, 51), Width = 300 };
        entryPanel.Controls.Add(txtAddress);

        // Row 3: GST | Save
        entryPanel.Controls.Add(lblGst = new Label { Text = "GST No.", Location = new Point(10, 95), AutoSize = true });
        txtGst = new TextBox { Location = new Point(70, 91), Width = 200 };
        entryPanel.Controls.Add(txtGst);

        btnSave = new Button
        {
            Text      = "Save Supplier",
            Location  = new Point(700, 88),
            Size      = new Size(130, 32),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += new EventHandler(btnSave_Click);
        entryPanel.Controls.Add(btnSave);

        Controls.Add(entryPanel);

        // ── Supplier list ─────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Supplier List",
            Location  = new Point(20, 214),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvSuppliers = new DataGridView
        {
            Location  = new Point(20, 232),
            Size      = new Size(852, 262),
            ReadOnly  = true,
            AllowUserToAddRows        = false,
            BackgroundColor           = Color.White,
            BorderStyle               = BorderStyle.FixedSingle,
            RowHeadersVisible         = false,
            SelectionMode             = DataGridViewSelectionMode.FullRowSelect,
            CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor                 = Color.FromArgb(220, 220, 235),
            AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 80),
                Padding   = new Padding(4, 0, 4, 0)
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 249, 255) },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.Black,
            },
            ColumnHeadersHeight       = 32,
            EnableHeadersVisualStyles = false,
            RowTemplate               = { Height = 26 },
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(dgvSuppliers);

        lblSupplierName = new Label { Visible = false };
        lblMobile       = new Label { Visible = false };
        lblEmail        = new Label { Visible = false };
        lblAddress      = new Label { Visible = false };
    }
}
