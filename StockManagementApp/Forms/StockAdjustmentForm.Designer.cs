namespace StockManagementApp.Forms;

partial class StockAdjustmentForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblProduct;
    private System.Windows.Forms.ComboBox cmbProduct;
    private System.Windows.Forms.Label lblAdjustmentType;
    private System.Windows.Forms.ComboBox cmbAdjustmentType;
    private System.Windows.Forms.Label lblQuantity;
    private System.Windows.Forms.TextBox txtQuantity;
    private System.Windows.Forms.Label lblReason;
    private System.Windows.Forms.TextBox txtReason;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.DataGridView dgvAdjustments;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Stock Adjustment";
        this.Size = new System.Drawing.Size(860, 560);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        lblTitle = new Label
        {
            Text      = "Stock Adjustment",
            Location  = new Point(20, 14),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        Controls.Add(new Label
        {
            Text      = "Adjustment Entry",
            Location  = new Point(20, 50),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        var entryPanel = new Panel
        {
            Location    = new Point(20, 68),
            Size        = new Size(810, 96),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Row 1: Product | Type | Quantity
        entryPanel.Controls.Add(lblProduct = new Label { Text = "Product", Location = new Point(10, 15), AutoSize = true });
        cmbProduct = new ComboBox { Location = new Point(68, 11), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
        entryPanel.Controls.Add(cmbProduct);

        entryPanel.Controls.Add(lblAdjustmentType = new Label { Text = "Type", Location = new Point(364, 15), AutoSize = true });
        cmbAdjustmentType = new ComboBox { Location = new Point(400, 11), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbAdjustmentType.Items.AddRange(new object[] { "Increase", "Decrease" });
        cmbAdjustmentType.SelectedIndex = 0;
        entryPanel.Controls.Add(cmbAdjustmentType);

        entryPanel.Controls.Add(lblQuantity = new Label { Text = "Quantity", Location = new Point(536, 15), AutoSize = true });
        txtQuantity = new TextBox { Location = new Point(600, 11), Width = 100 };
        entryPanel.Controls.Add(txtQuantity);

        // Row 2: Reason | Save
        entryPanel.Controls.Add(lblReason = new Label { Text = "Reason", Location = new Point(10, 57), AutoSize = true });
        txtReason = new TextBox { Location = new Point(68, 53), Width = 530 };
        entryPanel.Controls.Add(txtReason);

        btnSave = new Button
        {
            Text      = "Save Adjustment",
            Location  = new Point(624, 50),
            Size      = new Size(144, 30),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += new EventHandler(btnSave_Click);
        entryPanel.Controls.Add(btnSave);

        Controls.Add(entryPanel);

        Controls.Add(new Label
        {
            Text      = "Adjustment History",
            Location  = new Point(20, 180),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvAdjustments = new DataGridView
        {
            Location  = new Point(20, 198),
            Size      = new Size(810, 316),
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
        Controls.Add(dgvAdjustments);
    }
}
