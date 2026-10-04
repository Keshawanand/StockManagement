namespace StockManagementApp.Forms;

partial class SalesForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblCustomer;
    private System.Windows.Forms.ComboBox cmbCustomer;
    private System.Windows.Forms.Label lblProduct;
    private System.Windows.Forms.ComboBox cmbProduct;
    private System.Windows.Forms.Label lblQuantity;
    private System.Windows.Forms.TextBox txtQuantity;
    private System.Windows.Forms.Label lblSellingPrice;
    private System.Windows.Forms.TextBox txtSellingPrice;
    private System.Windows.Forms.Label lblGst;
    private System.Windows.Forms.TextBox txtGst;
    private System.Windows.Forms.Label lblDiscount;
    private System.Windows.Forms.TextBox txtDiscount;
    private System.Windows.Forms.Label lblInvoiceType;
    private System.Windows.Forms.ComboBox cmbInvoiceType;
    private System.Windows.Forms.Button btnPrintInvoice;
    private System.Windows.Forms.Button btnPrintToPrinter;
    private System.Windows.Forms.Button btnAddLine;
    private System.Windows.Forms.Button btnRemoveLine;
    private System.Windows.Forms.DataGridView dgvInvoiceLines;
    private System.Windows.Forms.Label lblSubtotal;
    private System.Windows.Forms.TextBox txtSubtotal;
    private System.Windows.Forms.Label lblTaxTotal;
    private System.Windows.Forms.TextBox txtTaxTotal;
    private System.Windows.Forms.Label lblGrandTotal;
    private System.Windows.Forms.TextBox txtGrandTotal;
    private System.Windows.Forms.Label lblPaidAmount;
    private System.Windows.Forms.TextBox txtPaidAmount;
    private System.Windows.Forms.Label lblDueAmount;
    private System.Windows.Forms.TextBox txtDueAmount;
    private System.Windows.Forms.ComboBox cmbPaymentMethod;
    private System.Windows.Forms.Label lblPaymentMethod;
    private System.Windows.Forms.Button btnSaveInvoice;
    private System.Windows.Forms.DataGridView dgvSales;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Sales Module";
        this.Size = new System.Drawing.Size(1050, 860);
        this.MinimumSize = new Size(900, 750);
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 246, 250);

        // ── Title ─────────────────────────────────────────────────────────────
        lblTitle = new Label
        {
            Text      = "Sales Entry",
            Location  = new Point(20, 14),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        // ── Row 1: Customer | Product | Qty | [Add Line] ──────────────────────
        Controls.Add(lblCustomer = new Label { Text = "Customer", Location = new Point(20, 58), AutoSize = true });
        cmbCustomer = new ComboBox { Location = new Point(88, 54), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        Controls.Add(cmbCustomer);

        Controls.Add(lblProduct = new Label { Text = "Product", Location = new Point(306, 58), AutoSize = true });
        cmbProduct = new ComboBox { Location = new Point(360, 54), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        cmbProduct.SelectedIndexChanged += new EventHandler(cmbProduct_SelectedIndexChanged);
        Controls.Add(cmbProduct);

        Controls.Add(lblQuantity = new Label { Text = "Qty", Location = new Point(598, 58), AutoSize = true });
        txtQuantity = new TextBox { Location = new Point(626, 54), Width = 90, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        Controls.Add(txtQuantity);

        btnAddLine = new Button
        {
            Text      = "Add Line",
            Location  = new Point(900, 52),
            Size      = new Size(120, 30),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };
        btnAddLine.FlatAppearance.BorderSize = 0;
        btnAddLine.Click += new EventHandler(btnSave_Click);
        Controls.Add(btnAddLine);

        // ── Row 2: Price | GST | Discount | [Remove Line] ─────────────────────
        Controls.Add(lblSellingPrice = new Label { Text = "Price ₹", Location = new Point(20, 98), AutoSize = true });
        txtSellingPrice = new TextBox { Location = new Point(68, 94), Width = 120, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        Controls.Add(txtSellingPrice);

        Controls.Add(lblGst = new Label { Text = "GST %", Location = new Point(205, 98), AutoSize = true });
        txtGst = new TextBox { Location = new Point(253, 94), Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        Controls.Add(txtGst);

        Controls.Add(lblDiscount = new Label { Text = "Discount", Location = new Point(350, 98), AutoSize = true });
        txtDiscount = new TextBox { Location = new Point(412, 94), Width = 100, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        Controls.Add(txtDiscount);

        btnRemoveLine = new Button
        {
            Text      = "Remove Line",
            Location  = new Point(900, 92),
            Size      = new Size(120, 30),
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };
        btnRemoveLine.FlatAppearance.BorderSize = 0;
        btnRemoveLine.Click += new EventHandler((s, e) => {
            if (dgvInvoiceLines.CurrentRow != null) dgvInvoiceLines.Rows.RemoveAt(dgvInvoiceLines.CurrentRow.Index);
        });
        Controls.Add(btnRemoveLine);

        // ── Row 3: Invoice type | Print buttons ───────────────────────────────
        Controls.Add(lblInvoiceType = new Label { Text = "Invoice", Location = new Point(20, 140), AutoSize = true });
        cmbInvoiceType = new ComboBox { Location = new Point(68, 136), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbInvoiceType.Items.AddRange(new object[] { "A4", "Thermal" });
        cmbInvoiceType.SelectedIndex = 0;
        Controls.Add(cmbInvoiceType);

        btnPrintInvoice = new Button
        {
            Text      = "Print Preview",
            Location  = new Point(206, 134),
            Size      = new Size(120, 30),
            BackColor = Color.FromArgb(99, 102, 241),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnPrintInvoice.FlatAppearance.BorderSize = 0;
        btnPrintInvoice.Click += new EventHandler(btnPrintInvoice_Click);
        Controls.Add(btnPrintInvoice);

        btnPrintToPrinter = new Button
        {
            Text      = "Print to Printer",
            Location  = new Point(334, 134),
            Size      = new Size(130, 30),
            BackColor = Color.FromArgb(99, 102, 241),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnPrintToPrinter.FlatAppearance.BorderSize = 0;
        btnPrintToPrinter.Click += new EventHandler(btnPrintToPrinter_Click);
        Controls.Add(btnPrintToPrinter);

        // ── Invoice lines grid ────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Invoice Lines",
            Location  = new Point(20, 180),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvInvoiceLines = new DataGridView
        {
            Location  = new Point(20, 198),
            Size      = new Size(1010, 310),
            ReadOnly  = false,
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
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId",   Visible = false });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product",   FillWeight = 160 });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity",    HeaderText = "Qty",       FillWeight = 40 });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice",   HeaderText = "Rate",      FillWeight = 70 });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "GstPercent",  HeaderText = "GST %",     FillWeight = 50 });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Discount",    HeaderText = "Discount",  FillWeight = 60 });
        dgvInvoiceLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "LineTotal",   HeaderText = "Amount",    ReadOnly = true, FillWeight = 80 });
        Controls.Add(dgvInvoiceLines);

        // ── Totals section ────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Invoice Totals",
            Location  = new Point(20, 522),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        Controls.Add(lblSubtotal = new Label { Text = "Subtotal", Location = new Point(20, 554), AutoSize = true });
        txtSubtotal = new TextBox { Location = new Point(80, 550), Width = 140, ReadOnly = true, BackColor = Color.FromArgb(248, 249, 255) };
        Controls.Add(txtSubtotal);

        Controls.Add(lblTaxTotal = new Label { Text = "Tax", Location = new Point(238, 554), AutoSize = true });
        txtTaxTotal = new TextBox { Location = new Point(264, 550), Width = 140, ReadOnly = true, BackColor = Color.FromArgb(248, 249, 255) };
        Controls.Add(txtTaxTotal);

        Controls.Add(lblGrandTotal = new Label { Text = "Grand Total", Location = new Point(422, 554), AutoSize = true });
        txtGrandTotal = new TextBox
        {
            Location  = new Point(506, 550),
            Width     = 160,
            ReadOnly  = true,
            BackColor = Color.FromArgb(240, 249, 244),
            Font      = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        Controls.Add(txtGrandTotal);

        Controls.Add(lblPaidAmount = new Label { Text = "Paid", Location = new Point(20, 596), AutoSize = true });
        txtPaidAmount = new TextBox { Location = new Point(52, 592), Width = 140, Text = "0" };
        txtPaidAmount.TextChanged += new EventHandler((s, e) => { RecalculateTotals(); });
        Controls.Add(txtPaidAmount);

        Controls.Add(lblDueAmount = new Label { Text = "Due", Location = new Point(210, 596), AutoSize = true });
        txtDueAmount = new TextBox { Location = new Point(240, 592), Width = 140, ReadOnly = true, BackColor = Color.FromArgb(255, 248, 248) };
        Controls.Add(txtDueAmount);

        Controls.Add(lblPaymentMethod = new Label { Text = "Payment", Location = new Point(398, 596), AutoSize = true });
        cmbPaymentMethod = new ComboBox { Location = new Point(458, 592), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "Card", "Bank Transfer", "Credit" });
        cmbPaymentMethod.SelectedIndex = 0;
        Controls.Add(cmbPaymentMethod);

        btnSaveInvoice = new Button
        {
            Text      = "Save Invoice",
            Location  = new Point(900, 588),
            Size      = new Size(130, 38),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };
        btnSaveInvoice.FlatAppearance.BorderSize = 0;
        btnSaveInvoice.Click += new EventHandler(btnSaveInvoice_Click);
        Controls.Add(btnSaveInvoice);

        // ── Recent Sales grid ─────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Recent Sales",
            Location  = new Point(20, 640),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        dgvSales = new DataGridView
        {
            Location  = new Point(20, 658),
            Size      = new Size(1010, 150),
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
            ColumnHeadersHeight       = 30,
            EnableHeadersVisualStyles = false,
            RowTemplate               = { Height = 24 },
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(dgvSales);
    }
}
