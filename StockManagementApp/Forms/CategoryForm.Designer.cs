namespace StockManagementApp.Forms;

partial class CategoryForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblCategoryName;
    private System.Windows.Forms.TextBox txtCategoryName;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.DataGridView dgvCategories;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Category Management";
        this.Size = new System.Drawing.Size(680, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        lblTitle = new Label
        {
            Text      = "Category Management",
            Location  = new Point(20, 14),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        Controls.Add(new Label
        {
            Text      = "Add Category",
            Location  = new Point(20, 50),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        var entryPanel = new Panel
        {
            Location    = new Point(20, 68),
            Size        = new Size(632, 60),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        entryPanel.Controls.Add(lblCategoryName = new Label { Text = "Category Name", Location = new Point(10, 18), AutoSize = true });
        txtCategoryName = new TextBox { Location = new Point(110, 14), Width = 280 };
        entryPanel.Controls.Add(txtCategoryName);

        btnSave = new Button
        {
            Text      = "Add Category",
            Location  = new Point(480, 11),
            Size      = new Size(130, 30),
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
            Text      = "Categories",
            Location  = new Point(20, 144),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvCategories = new DataGridView
        {
            Location  = new Point(20, 162),
            Size      = new Size(632, 270),
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
        Controls.Add(dgvCategories);
    }
}
