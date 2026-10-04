namespace StockManagementApp.Forms;

partial class ReportsForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.ComboBox cmbReportType;
    private System.Windows.Forms.Button btnGenerate;
    private System.Windows.Forms.DataGridView dgvReport;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Reports";
        this.Size = new System.Drawing.Size(1000, 640);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        lblTitle = new Label
        {
            Text      = "Reports",
            Location  = new Point(20, 14),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        Controls.Add(new Label
        {
            Text      = "Report Type",
            Location  = new Point(20, 56),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        cmbReportType = new ComboBox { Location = new Point(20, 76), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbReportType.Items.AddRange(new object[] { "Product Stock", "Sales Summary", "Purchase Summary", "Low Stock" });
        cmbReportType.SelectedIndex = 0;
        Controls.Add(cmbReportType);

        btnGenerate = new Button
        {
            Text      = "Generate Report",
            Location  = new Point(316, 74),
            Size      = new Size(150, 30),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += new EventHandler(btnGenerate_Click);
        Controls.Add(btnGenerate);

        Controls.Add(new Label
        {
            Text      = "Results",
            Location  = new Point(20, 120),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvReport = new DataGridView
        {
            Location  = new Point(20, 138),
            Size      = new Size(952, 452),
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
        Controls.Add(dgvReport);
    }
}
