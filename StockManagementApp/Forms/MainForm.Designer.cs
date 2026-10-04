namespace StockManagementApp.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem menuInventory;
    private System.Windows.Forms.ToolStripMenuItem menuPayments;
    private System.Windows.Forms.ToolStripMenuItem menuTransactions;
    private System.Windows.Forms.ToolStripMenuItem menuReturns;
    private System.Windows.Forms.ToolStripMenuItem menuAdjustments;
    private System.Windows.Forms.ToolStripMenuItem menuSuppliers;
    private System.Windows.Forms.ToolStripMenuItem menuCustomers;
    private System.Windows.Forms.ToolStripMenuItem menuCategories;
    private System.Windows.Forms.ToolStripMenuItem menuPurchases;
    private System.Windows.Forms.ToolStripMenuItem menuSales;
    private System.Windows.Forms.ToolStripMenuItem menuStockAdjustments;
    private System.Windows.Forms.ToolStripMenuItem menuSalesReturns;
    private System.Windows.Forms.ToolStripMenuItem menuPurchaseReturns;
    private System.Windows.Forms.ToolStripMenuItem menuReports;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblTotalProducts;
    private System.Windows.Forms.Label lblTotalCustomers;
    private System.Windows.Forms.Label lblTotalSuppliers;
    private System.Windows.Forms.Label lblTotalSales;
    private System.Windows.Forms.Label lblTotalPurchases;
    private System.Windows.Forms.Label lblStockValue;
    private System.Windows.Forms.Label lblLowStock;
    private System.Windows.Forms.Label lblOutOfStock;
    private System.Windows.Forms.TextBox txtProductName;
    private System.Windows.Forms.TextBox txtProductCode;
    private System.Windows.Forms.TextBox txtBarcode;
    private System.Windows.Forms.TextBox txtCategory;
    private System.Windows.Forms.TextBox txtBrand;
    private System.Windows.Forms.TextBox txtUnit;
    private System.Windows.Forms.TextBox txtPurchasePrice;
    private System.Windows.Forms.TextBox txtSellingPrice;
    private System.Windows.Forms.TextBox txtGst;
    private System.Windows.Forms.TextBox txtReorderLevel;
    private System.Windows.Forms.TextBox txtCurrentStock;
    private System.Windows.Forms.TextBox txtDescription;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.Button btnAddProduct;
    private System.Windows.Forms.Button btnUpdateProduct;
    private System.Windows.Forms.Button btnDeleteProduct;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.DataGridView dgvProducts;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Stock Management System";
        this.Size = new System.Drawing.Size(1280, 830);
        this.MinimumSize = new System.Drawing.Size(1100, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(245, 246, 250);

        // ── Menu ──────────────────────────────────────────────────────────────
        menuStrip = new MenuStrip { BackColor = Color.White, Padding = new Padding(4, 2, 0, 2) };
        menuInventory        = new ToolStripMenuItem("Inventory");
        menuTransactions     = new ToolStripMenuItem("Transactions");
        menuReturns          = new ToolStripMenuItem("Returns");
        menuAdjustments      = new ToolStripMenuItem("Adjustments");
        menuSuppliers        = new ToolStripMenuItem("Suppliers");
        menuCustomers        = new ToolStripMenuItem("Customers");
        menuCategories       = new ToolStripMenuItem("Categories");
        menuPurchases        = new ToolStripMenuItem("Purchases");
        menuSales            = new ToolStripMenuItem("Sales");
        menuStockAdjustments = new ToolStripMenuItem("Stock Adjustments");
        menuSalesReturns     = new ToolStripMenuItem("Sales Returns");
        menuPurchaseReturns  = new ToolStripMenuItem("Purchase Returns");
        menuReports          = new ToolStripMenuItem("Reports");

        menuInventory.DropDownItems.AddRange(new ToolStripItem[] { menuSuppliers, menuCustomers, menuCategories });
        menuTransactions.DropDownItems.AddRange(new ToolStripItem[] { menuPurchases, menuSales });
        menuAdjustments.DropDownItems.Add(menuStockAdjustments);
        menuReturns.DropDownItems.AddRange(new ToolStripItem[] { menuSalesReturns, menuPurchaseReturns });

        menuPayments = new ToolStripMenuItem("Payments");
        var menuCustomerPayments = new ToolStripMenuItem("Customer Payments");
        var menuSupplierPayments = new ToolStripMenuItem("Supplier Payments");
        menuCustomerPayments.Click += menuCustomerPayments_Click;
        menuSupplierPayments.Click += menuSupplierPayments_Click;
        menuPayments.DropDownItems.AddRange(new ToolStripItem[] { menuCustomerPayments, menuSupplierPayments });

        menuStrip.Items.AddRange(new ToolStripItem[] { menuInventory, menuTransactions, menuPayments, menuAdjustments, menuReturns, menuReports });
        menuSuppliers.Click        += menuSuppliers_Click;
        menuCustomers.Click        += menuCustomers_Click;
        menuCategories.Click       += menuCategories_Click;
        menuPurchases.Click        += menuPurchases_Click;
        menuSales.Click            += menuSales_Click;
        menuStockAdjustments.Click += menuStockAdjustments_Click;
        menuSalesReturns.Click     += menuSalesReturns_Click;
        menuPurchaseReturns.Click  += menuPurchaseReturns_Click;
        menuReports.Click          += menuReports_Click;
        Controls.Add(menuStrip);

        // ── Page title ────────────────────────────────────────────────────────
        lblTitle = new Label
        {
            Text      = "Stock Management Dashboard",
            Location  = new Point(20, 36),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 60)
        };
        Controls.Add(lblTitle);

        // ── Stat tiles  (8 × 148 px + 7 × 8 px gap = 1240 px) ───────────────
        var statsPanel = new Panel { Location = new Point(20, 76), Size = new Size(1240, 82), BackColor = Color.Transparent };

        (string text, Color color)[] tileData =
        {
            ("Products",     Color.FromArgb( 59, 130, 246)),
            ("Customers",    Color.FromArgb( 16, 185, 129)),
            ("Suppliers",    Color.FromArgb( 14, 165, 233)),
            ("Sales",        Color.FromArgb(249, 115,  22)),
            ("Purchases",    Color.FromArgb(139,  92, 246)),
            ("Stock Qty",    Color.FromArgb( 99, 102, 241)),
            ("Low Stock",    Color.FromArgb(234, 179,   8)),
            ("Out of Stock", Color.FromArgb(239,  68,  68)),
        };

        var valLabels = new Label[8];
        const int TileW = 148, TileH = 80, TileGap = 8;
        for (int i = 0; i < tileData.Length; i++)
        {
            var tile = new Panel
            {
                Location  = new Point(i * (TileW + TileGap), 0),
                Size      = new Size(TileW, TileH),
                BackColor = tileData[i].color
            };
            tile.Controls.Add(new Label
            {
                Text      = tileData[i].text,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
                Location  = new Point(12, 10),
                AutoSize  = true
            });
            var vl = new Label
            {
                Text      = "0",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                Location  = new Point(12, 32),
                AutoSize  = true
            };
            valLabels[i] = vl;
            tile.Controls.Add(vl);
            statsPanel.Controls.Add(tile);
        }
        lblTotalProducts  = valLabels[0];
        lblTotalCustomers = valLabels[1];
        lblTotalSuppliers = valLabels[2];
        lblTotalSales     = valLabels[3];
        lblTotalPurchases = valLabels[4];
        lblStockValue     = valLabels[5];
        lblLowStock       = valLabels[6];
        lblOutOfStock     = valLabels[7];
        Controls.Add(statsPanel);

        // ── Section label ─────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Product Management",
            Location  = new Point(20, 174),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        // ── Product form  (5-column layout) ───────────────────────────────────
        // Column start x values; each column = 248 px (label ~90 + field ~150 + gap 8)
        // col0=10, col1=258, col2=506, col3=754, col4=1002
        var formPanel = new Panel
        {
            Location    = new Point(20, 196),
            Size        = new Size(1240, 182),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        void AddLF(string ltext, int x, int y, int lw, TextBox tb)
        {
            formPanel.Controls.Add(new Label
            {
                Text      = ltext,
                Location  = new Point(x, y + 3),
                Width     = lw,
                TextAlign = ContentAlignment.MiddleRight
            });
            tb.Location = new Point(x + lw + 4, y);
            formPanel.Controls.Add(tb);
        }

        // Row 1 — y=14
        AddLF("Product Name", 10,  14, 90, txtProductName  = new TextBox { Width = 148 });
        AddLF("Code",        258,  14, 50, txtProductCode  = new TextBox { Width = 188 });
        AddLF("Barcode",     506,  14, 58, txtBarcode      = new TextBox { Width = 180 });
        AddLF("Category",    754,  14, 65, txtCategory     = new TextBox { Width = 173 });
        AddLF("Brand",      1002,  14, 50, txtBrand        = new TextBox { Width = 178 });

        // Row 2 — y=52
        AddLF("Unit",        10,  52, 90, txtUnit          = new TextBox { Width = 148 });
        AddLF("Purchase ₹", 258,  52, 70, txtPurchasePrice = new TextBox { Width = 168 });
        AddLF("Selling ₹",  506,  52, 65, txtSellingPrice  = new TextBox { Width = 173 });
        AddLF("GST %",       754,  52, 50, txtGst           = new TextBox { Width = 80  });
        AddLF("Reorder",    1002,  52, 58, txtReorderLevel  = new TextBox { Width = 68  });
        AddLF("Stock",      1140,  52, 42, txtCurrentStock  = new TextBox { Width = 68  });

        // Row 3 — y=90  Description spanning most of the width
        AddLF("Description", 10,  90, 90, txtDescription = new TextBox { Width = 1040 });

        // Buttons — right-aligned at y=134
        btnAddProduct = new Button
        {
            Text      = "Add Product",
            Location  = new Point(852, 132),
            Size      = new Size(115, 32),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnAddProduct.FlatAppearance.BorderSize = 0;

        btnUpdateProduct = new Button
        {
            Text      = "Update",
            Location  = new Point(975, 132),
            Size      = new Size(115, 32),
            BackColor = Color.FromArgb(249, 115, 22),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnUpdateProduct.FlatAppearance.BorderSize = 0;

        btnDeleteProduct = new Button
        {
            Text      = "Delete",
            Location  = new Point(1098, 132),
            Size      = new Size(115, 32),
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnDeleteProduct.FlatAppearance.BorderSize = 0;

        btnAddProduct.Click    += btnAddProduct_Click;
        btnUpdateProduct.Click += btnUpdateProduct_Click;
        btnDeleteProduct.Click += btnDeleteProduct_Click;
        formPanel.Controls.AddRange(new Control[] { btnAddProduct, btnUpdateProduct, btnDeleteProduct });

        Controls.Add(formPanel);

        // ── Search bar ────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Products",
            Location  = new Point(20, 394),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 110)
        });

        txtSearch = new TextBox { Location = new Point(20, 416), Width = 380 };
        btnSearch = new Button
        {
            Text      = "Search",
            Location  = new Point(408, 414),
            Size      = new Size(90, 26),
            BackColor = Color.FromArgb(99, 102, 241),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += btnSearch_Click;
        Controls.Add(txtSearch);
        Controls.Add(btnSearch);

        // ── Products grid ─────────────────────────────────────────────────────
        dgvProducts = new DataGridView
        {
            Location            = new Point(20, 450),
            Size                = new Size(1240, 320),
            ReadOnly            = true,
            AllowUserToAddRows  = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor     = Color.White,
            BorderStyle         = BorderStyle.FixedSingle,
            RowHeadersVisible   = false,
            SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
            CellBorderStyle     = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor           = Color.FromArgb(220, 220, 235),

            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 255)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(219, 234, 254),
                SelectionForeColor = Color.Black,
                Padding            = new Padding(2, 0, 2, 0)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 80),
                Padding   = new Padding(4, 0, 4, 0)
            },
            ColumnHeadersHeight        = 32,
            EnableHeadersVisualStyles  = false,
            RowTemplate                = { Height = 26 }
        };
        dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
        Controls.Add(dgvProducts);
    }
}
