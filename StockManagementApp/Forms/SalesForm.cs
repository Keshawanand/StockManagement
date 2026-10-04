using StockManagementApp.BusinessLogic;
using StockManagementApp.Helpers;
using System.Data;

namespace StockManagementApp.Forms;

public partial class SalesForm : Form
{
    private readonly SalesService _service = new();
    private readonly CustomerService _customerService = new();
    private readonly ProductService _productService = new();

    public SalesForm()
    {
        InitializeComponent();
        LoadCustomers();
        LoadProducts();
        LoadSales();

        this.Resize += (s, e) => AdjustQuantityWidth();
        this.Shown += (s, e) => AdjustQuantityWidth();
        AdjustQuantityWidth();
    }

    private void AdjustQuantityWidth()
    {
        if (txtQuantity == null) return;
        var rightEdge = btnAddLine != null ? btnAddLine.Left : (this.ClientSize.Width - 120);
        var newWidth = rightEdge - txtQuantity.Left - 12;
        if (newWidth < 60) newWidth = 60;
        txtQuantity.Width = newWidth;
    }

    private void cmbProduct_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbProduct.SelectedValue == null) return;
        var pid = Convert.ToInt32(cmbProduct.SelectedValue);
        var tbl = _productService.GetProductById(pid);
        if (tbl.Rows.Count == 0) return;
        var row = tbl.Rows[0];
        txtSellingPrice.Text = Convert.ToDecimal(row["SellingPrice"]).ToString("0.00");
        txtGst.Text = Convert.ToDecimal(row["GstPercent"]).ToString("0.00");
    }

    private void RecalculateTotals()
    {
        decimal subtotal = 0m, taxTotal = 0m, grand = 0m;
        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            var qty = Convert.ToDecimal(r.Cells["Quantity"].Value ?? 0);
            var rate = Convert.ToDecimal(r.Cells["UnitPrice"].Value ?? 0m);
            var gst = Convert.ToDecimal(r.Cells["GstPercent"].Value ?? 0m);
            var disc = Convert.ToDecimal(r.Cells["Discount"].Value ?? 0m);
            var lineBase = qty * rate;
            var lineTax = Math.Round(lineBase * gst / 100m, 2);
            var lineTotal = lineBase + lineTax - disc;
            subtotal += lineBase - disc;
            taxTotal += lineTax;
            grand += lineTotal;
            r.Cells["LineTotal"].Value = lineTotal.ToString("0.00");
        }

        txtSubtotal.Text = subtotal.ToString("0.00");
        txtTaxTotal.Text = taxTotal.ToString("0.00");
        txtGrandTotal.Text = grand.ToString("0.00");
        var paid = decimal.TryParse(txtPaidAmount.Text, out var p) ? p : 0m;
        txtDueAmount.Text = (grand - paid).ToString("0.00");
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        // Add current selection as a line
        if (cmbProduct.SelectedValue == null)
        {
            MessageBox.Show("Select a product first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtQuantity.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("Enter a valid quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pid = Convert.ToInt32(cmbProduct.SelectedValue);
        var prodTbl = _productService.GetProductById(pid);
        if (prodTbl.Rows.Count == 0) { MessageBox.Show("Product not found."); return; }
        var currentStock = Convert.ToInt32(prodTbl.Rows[0]["CurrentStock"] ?? 0);
        if (qty > currentStock) { MessageBox.Show("Insufficient stock.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var rate = decimal.TryParse(txtSellingPrice.Text, out var rrate) ? rrate : Convert.ToDecimal(prodTbl.Rows[0]["SellingPrice"]);
        var gst = decimal.TryParse(txtGst.Text, out var rgst) ? rgst : Convert.ToDecimal(prodTbl.Rows[0]["GstPercent"]);
        var disc = decimal.TryParse(txtDiscount.Text, out var rdisc) ? rdisc : 0m;

        var lineBase = qty * rate;
        var lineTax = Math.Round(lineBase * gst / 100m, 2);
        var lineTotal = lineBase + lineTax - disc;

        dgvInvoiceLines.Rows.Add(pid, cmbProduct.Text, qty, rate.ToString("0.00"), gst.ToString("0.00"), disc.ToString("0.00"), lineTotal.ToString("0.00"));
        RecalculateTotals();
    }

    private void btnSaveInvoice_Click(object? sender, EventArgs e)
    {
        if (cmbCustomer.SelectedValue == null)
        {
            MessageBox.Show("Select a customer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (dgvInvoiceLines.Rows.Count == 0)
        {
            MessageBox.Show("Add at least one invoice line.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var items = new List<(int ProductId, int Quantity, decimal SellingPrice, decimal GstPercent, decimal Discount)>();
        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            var pid = Convert.ToInt32(r.Cells["ProductId"].Value);
            var qty = Convert.ToInt32(r.Cells["Quantity"].Value);
            var rate = Convert.ToDecimal(r.Cells["UnitPrice"].Value);
            var gst = Convert.ToDecimal(r.Cells["GstPercent"].Value);
            var disc = Convert.ToDecimal(r.Cells["Discount"].Value);
            items.Add((pid, qty, rate, gst, disc));
        }

        var paid = decimal.TryParse(txtPaidAmount.Text, out var paidAmt) ? paidAmt : 0m;

        try
        {
            _service.AddSalesInvoice(Convert.ToInt32(cmbCustomer.SelectedValue), items, paid);
            MessageBox.Show("Invoice saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dgvInvoiceLines.Rows.Clear();
            txtPaidAmount.Text = "0";
            RecalculateTotals();
            LoadSales();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadCustomers()
    {
        var customers = _customerService.GetCustomers();
        cmbCustomer.DisplayMember = "CustomerName";
        cmbCustomer.ValueMember = "CustomerId";
        cmbCustomer.DataSource = customers;
    }

    private void LoadProducts()
    {
        var products = _productService.GetProducts();
        cmbProduct.DisplayMember = "ProductName";
        cmbProduct.ValueMember = "ProductId";
        cmbProduct.DataSource = products;
    }

    private void LoadSales()
    {
        dgvSales.DataSource = _service.GetSales();
    }

    

    private bool TryGetSelectedSalesInvoice(out InvoiceData invoice)
    {
        invoice = null!;

        if (dgvSales.CurrentRow?.DataBoundItem is not DataRowView row)
            return false;

        if (row["InvoiceNumber"] == DBNull.Value || string.IsNullOrEmpty(row["InvoiceNumber"]?.ToString()))
            return false;

        var invoiceNumber = row["InvoiceNumber"].ToString()!;
        var customerId = Convert.ToInt32(row["CustomerId"]);
        var customerTable = _customerService.GetCustomerById(customerId);

        if (customerTable.Rows.Count == 0)
            return false;

        var customerRow = customerTable.Rows[0];
        var itemsTable = _service.GetInvoiceItems(invoiceNumber);

        invoice = new InvoiceData
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = Convert.ToDateTime(row["SaleDate"]),
            TransactionType = "Sales",
            PartyName = customerRow["CustomerName"].ToString() ?? string.Empty,
            PartyAddress = customerRow["Address"].ToString() ?? string.Empty,
            PartyMobile = customerRow["Mobile"].ToString() ?? string.Empty,
            PartyGstNumber = customerRow["GstNumber"].ToString() ?? string.Empty,
            InvoiceType = cmbInvoiceType.SelectedItem?.ToString() ?? "A4"
        };

        if (itemsTable.Rows.Count > 0)
        {
            foreach (DataRow ir in itemsTable.Rows)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    Description = ir["ProductName"].ToString() ?? string.Empty,
                    Quantity = Convert.ToInt32(ir["Quantity"]),
                    UnitPrice = Convert.ToDecimal(ir["SellingPrice"]),
                    GstPercent = Convert.ToDecimal(ir["GstPercent"]),
                    Discount = Convert.ToDecimal(ir["Discount"])
                });
            }
        }
        else
        {
            // fallback to the single-row data from the legacy Sales table
            invoice.Items.Add(new InvoiceItem
            {
                Description = row["ProductName"].ToString() ?? string.Empty,
                Quantity = Convert.ToInt32(row["Quantity"]),
                UnitPrice = Convert.ToDecimal(row["SellingPrice"]),
                GstPercent = Convert.ToDecimal(row["GstPercent"]),
                Discount = Convert.ToDecimal(row["Discount"])
            });
        }

        return invoice.Items.Count > 0;
    }

    private bool TryBuildCurrentInvoice(out InvoiceData invoice)
    {
        invoice = null!;
        if (dgvInvoiceLines.Rows.Count == 0 || cmbCustomer.SelectedValue == null)
            return false;

        var custId = Convert.ToInt32(cmbCustomer.SelectedValue);
        var custTbl = _customerService.GetCustomerById(custId);
        if (custTbl.Rows.Count == 0) return false;
        var cr = custTbl.Rows[0];

        invoice = new InvoiceData
        {
            InvoiceNumber   = $"PREVIEW-{DateTime.Now:HHmmss}",
            InvoiceDate     = DateTime.Now,
            TransactionType = "Sales",
            PartyName       = cr["CustomerName"].ToString() ?? string.Empty,
            PartyAddress    = cr["Address"].ToString() ?? string.Empty,
            PartyMobile     = cr["Mobile"].ToString() ?? string.Empty,
            PartyGstNumber  = cr["GstNumber"].ToString() ?? string.Empty,
            InvoiceType     = cmbInvoiceType.SelectedItem?.ToString() ?? "A4"
        };

        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            invoice.Items.Add(new InvoiceItem
            {
                Description = r.Cells["ProductName"].Value?.ToString() ?? string.Empty,
                Quantity    = Convert.ToInt32(r.Cells["Quantity"].Value ?? 0),
                UnitPrice   = Convert.ToDecimal(r.Cells["UnitPrice"].Value ?? 0m),
                GstPercent  = Convert.ToDecimal(r.Cells["GstPercent"].Value ?? 0m),
                Discount    = Convert.ToDecimal(r.Cells["Discount"].Value ?? 0m)
            });
        }
        return invoice.Items.Count > 0;
    }

    private void btnPrintInvoice_Click(object sender, EventArgs e)
    {
        if (TryBuildCurrentInvoice(out var invoice) || TryGetSelectedSalesInvoice(out invoice))
            InvoicePrinter.ShowPrintPreview(invoice);
        else
            MessageBox.Show("Add items to the invoice or select a saved invoice from the grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void btnPrintToPrinter_Click(object sender, EventArgs e)
    {
        if (TryBuildCurrentInvoice(out var invoice) || TryGetSelectedSalesInvoice(out invoice))
            InvoicePrinter.PrintInvoice(invoice);
        else
            MessageBox.Show("Add items to the invoice or select a saved invoice from the grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
