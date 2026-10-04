using System.Data;
using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class MainForm : Form
{
    private readonly ProductService _productService = new();
    private readonly TransactionService _transactionService = new();

    public MainForm()
    {
        InitializeComponent();
        LoadDashboard();
        LoadProducts();
    }

    private void menuSuppliers_Click(object sender, EventArgs e)
    {
        var form = new SupplierForm();
        form.ShowDialog(this);
    }

    private void menuCustomers_Click(object sender, EventArgs e)
    {
        var form = new CustomerForm();
        form.ShowDialog(this);
    }

    private void menuPurchases_Click(object sender, EventArgs e)
    {
        var form = new PurchaseForm();
        form.ShowDialog(this);
        LoadDashboard();
        LoadProducts();
    }

    private void menuSales_Click(object sender, EventArgs e)
    {
        var form = new SalesForm();
        form.ShowDialog(this);
        LoadDashboard();
        LoadProducts();
    }

    private void menuCategories_Click(object sender, EventArgs e)
    {
        var form = new CategoryForm();
        form.ShowDialog(this);
    }

    private void menuStockAdjustments_Click(object sender, EventArgs e)
    {
        var form = new StockAdjustmentForm();
        form.ShowDialog(this);
    }

    private void menuSalesReturns_Click(object sender, EventArgs e)
    {
        var form = new SalesReturnForm();
        form.ShowDialog(this);
    }

    private void menuPurchaseReturns_Click(object sender, EventArgs e)
    {
        var form = new PurchaseReturnForm();
        form.ShowDialog(this);
    }

    private void menuReports_Click(object sender, EventArgs e)
    {
        var form = new ReportsForm();
        form.ShowDialog(this);
    }

    private void menuCustomerPayments_Click(object sender, EventArgs e)
    {
        var form = new CustomerPaymentForm();
        form.ShowDialog(this);
    }

    private void menuSupplierPayments_Click(object sender, EventArgs e)
    {
        var form = new SupplierPaymentForm();
        form.ShowDialog(this);
    }

    private void LoadDashboard()
    {
        var table = _transactionService.GetDashboardSummary();
        if (table.Rows.Count > 0)
        {
            var row = table.Rows[0];
            lblTotalProducts.Text = row["TotalProducts"].ToString();
            lblTotalCustomers.Text = row["TotalCustomers"].ToString();
            lblTotalSuppliers.Text = row["TotalSuppliers"].ToString();
            lblTotalSales.Text = row["TotalSales"].ToString();
            lblTotalPurchases.Text = row["TotalPurchases"].ToString();
            lblStockValue.Text = row["TotalStockQuantity"].ToString();
            lblLowStock.Text = row["LowStockProducts"].ToString();
            lblOutOfStock.Text = row["OutOfStockProducts"].ToString();
        }
    }

    private void LoadProducts()
    {
        dgvProducts.DataSource = _productService.GetProducts();
        SetProductGridColumns();
    }

    private void SetProductGridColumns()
    {
        if (dgvProducts.Columns.Count == 0) return;

        var headers = new Dictionary<string, (string Header, int Width, bool Visible)>
        {
            ["ProductId"]    = ("ID",             40,  false),
            ["ProductName"]  = ("Product Name",  180,  true),
            ["ProductCode"]  = ("Code",           80,  true),
            ["Barcode"]      = ("Barcode",        90,  true),
            ["CategoryName"] = ("Category",      110,  true),
            ["Brand"]        = ("Brand",          90,  true),
            ["Unit"]         = ("Unit",           55,  true),
            ["PurchasePrice"]= ("Purchase ₹",     90,  true),
            ["SellingPrice"] = ("Selling ₹",      90,  true),
            ["GstPercent"]   = ("GST %",          60,  true),
            ["ReorderLevel"] = ("Reorder",        60,  true),
            ["CurrentStock"] = ("Stock",          55,  true),
            ["Description"]  = ("Description",   200,  true),
        };

        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        foreach (DataGridViewColumn col in dgvProducts.Columns)
        {
            if (headers.TryGetValue(col.Name, out var cfg))
            {
                col.HeaderText = cfg.Header;
                col.Width      = cfg.Width;
                col.Visible    = cfg.Visible;
            }
        }
        if (dgvProducts.Columns.Contains("Description"))
            dgvProducts.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private void btnAddProduct_Click(object sender, EventArgs e)
    {
        try
        {
            var productName = txtProductName.Text.Trim();
            var productCode = txtProductCode.Text.Trim();
            var barcode = txtBarcode.Text.Trim();
            var category = txtCategory.Text.Trim();
            var brand = txtBrand.Text.Trim();
            var unit = txtUnit.Text.Trim();
            var purchasePrice = decimal.Parse(txtPurchasePrice.Text);
            var sellingPrice = decimal.Parse(txtSellingPrice.Text);
            var gst = decimal.Parse(txtGst.Text);
            var reorderLevel = int.Parse(txtReorderLevel.Text);
            var currentStock = int.Parse(txtCurrentStock.Text);
            var description = txtDescription.Text.Trim();

            _productService.AddProduct(productName, productCode, barcode, category, brand, unit, purchasePrice, sellingPrice, gst, reorderLevel, currentStock, description);
            LoadProducts();
            MessageBox.Show("Product added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        dgvProducts.DataSource = _productService.SearchProducts(txtSearch.Text.Trim());
    }

    private void btnUpdateProduct_Click(object sender, EventArgs e)
    {
        if (dgvProducts.CurrentRow == null)
            return;

        try
        {
            var productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductId"].Value);
            _productService.UpdateProduct(productId, txtProductName.Text.Trim(), txtProductCode.Text.Trim(), txtBarcode.Text.Trim(), txtCategory.Text.Trim(), txtBrand.Text.Trim(), txtUnit.Text.Trim(), decimal.Parse(txtPurchasePrice.Text), decimal.Parse(txtSellingPrice.Text), decimal.Parse(txtGst.Text), int.Parse(txtReorderLevel.Text), int.Parse(txtCurrentStock.Text), txtDescription.Text.Trim());
            LoadProducts();
            MessageBox.Show("Product updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnDeleteProduct_Click(object sender, EventArgs e)
    {
        if (dgvProducts.CurrentRow == null)
            return;

        var confirm = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm == DialogResult.Yes)
        {
            try
            {
                var productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductId"].Value);
                _productService.DeleteProduct(productId);
                LoadProducts();
                MessageBox.Show("Product deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void dgvProducts_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvProducts.CurrentRow == null)
            return;

        txtProductName.Text = dgvProducts.CurrentRow.Cells["ProductName"].Value?.ToString();
        txtProductCode.Text = dgvProducts.CurrentRow.Cells["ProductCode"].Value?.ToString();
        txtBarcode.Text = dgvProducts.CurrentRow.Cells["Barcode"].Value?.ToString();
        txtCategory.Text = dgvProducts.CurrentRow.Cells["CategoryName"].Value?.ToString();
        txtBrand.Text = dgvProducts.CurrentRow.Cells["Brand"].Value?.ToString();
        txtUnit.Text = dgvProducts.CurrentRow.Cells["Unit"].Value?.ToString();
        txtPurchasePrice.Text = dgvProducts.CurrentRow.Cells["PurchasePrice"].Value?.ToString();
        txtSellingPrice.Text = dgvProducts.CurrentRow.Cells["SellingPrice"].Value?.ToString();
        txtGst.Text = dgvProducts.CurrentRow.Cells["GstPercent"].Value?.ToString();
        txtReorderLevel.Text = dgvProducts.CurrentRow.Cells["ReorderLevel"].Value?.ToString();
        txtCurrentStock.Text = dgvProducts.CurrentRow.Cells["CurrentStock"].Value?.ToString();
        txtDescription.Text = dgvProducts.CurrentRow.Cells["Description"].Value?.ToString();
    }
}
