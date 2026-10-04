using StockManagementApp.BusinessLogic;
using StockManagementApp.Helpers;
using System.Data;

namespace StockManagementApp.Forms;

public partial class PurchaseForm : Form
{
    private readonly PurchaseService _purchaseService = new();
    private readonly SupplierService _supplierService = new();
    private readonly ProductService _productService = new();

    public PurchaseForm()
    {
        InitializeComponent();
        LoadSuppliers();
        LoadProducts();
        LoadPurchases();
    }

    private void LoadSuppliers()
    {
        var suppliers = _supplierService.GetSuppliers();
        cmbSupplier.DisplayMember = "SupplierName";
        cmbSupplier.ValueMember = "SupplierId";
        cmbSupplier.DataSource = suppliers;
    }

    private void LoadProducts()
    {
        var products = _productService.GetProducts();
        cmbProduct.DisplayMember = "ProductName";
        cmbProduct.ValueMember = "ProductId";
        cmbProduct.DataSource = products;
    }

    private void LoadPurchases()
    {
        dgvPurchases.DataSource = _purchaseService.GetPurchases();
    }

    private void cmbProduct_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbProduct.SelectedValue == null) return;
        var pid = Convert.ToInt32(cmbProduct.SelectedValue);
        var tbl = _productService.GetProductById(pid);
        if (tbl.Rows.Count == 0) return;
        var row = tbl.Rows[0];
        txtPurchasePrice.Text = Convert.ToDecimal(row["PurchasePrice"]).ToString("0.00");
        txtGst.Text = Convert.ToDecimal(row["GstPercent"]).ToString("0.00");
    }

    private void RecalculateTotals()
    {
        decimal subtotal = 0m, taxTotal = 0m, grand = 0m;
        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            var qty = Convert.ToDecimal(r.Cells["Quantity"].Value ?? 0);
            var price = Convert.ToDecimal(r.Cells["PurchasePrice"].Value ?? 0m);
            var gst = Convert.ToDecimal(r.Cells["GstPercent"].Value ?? 0m);
            var disc = Convert.ToDecimal(r.Cells["Discount"].Value ?? 0m);
            var lineBase = qty * price;
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

    private void btnAddLine_Click(object? sender, EventArgs e)
    {
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

        var price = decimal.TryParse(txtPurchasePrice.Text, out var rp) ? rp : Convert.ToDecimal(prodTbl.Rows[0]["PurchasePrice"]);
        var gst = decimal.TryParse(txtGst.Text, out var rg) ? rg : Convert.ToDecimal(prodTbl.Rows[0]["GstPercent"]);
        var disc = decimal.TryParse(txtDiscount.Text, out var rd) ? rd : 0m;

        var lineTotal = price * qty + Math.Round(price * qty * gst / 100m, 2) - disc;
        dgvInvoiceLines.Rows.Add(pid, cmbProduct.Text, qty, price.ToString("0.00"), gst.ToString("0.00"), disc.ToString("0.00"), lineTotal.ToString("0.00"));
        RecalculateTotals();

        txtQuantity.Text = string.Empty;
        txtDiscount.Text = string.Empty;
    }

    private void btnSaveInvoice_Click(object? sender, EventArgs e)
    {
        if (cmbSupplier.SelectedValue == null)
        {
            MessageBox.Show("Select a supplier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (dgvInvoiceLines.Rows.Count == 0)
        {
            MessageBox.Show("Add at least one invoice line.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var items = new List<(int ProductId, int Quantity, decimal PurchasePrice, decimal GstPercent, decimal Discount)>();
        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            items.Add((
                Convert.ToInt32(r.Cells["ProductId"].Value),
                Convert.ToInt32(r.Cells["Quantity"].Value),
                Convert.ToDecimal(r.Cells["PurchasePrice"].Value),
                Convert.ToDecimal(r.Cells["GstPercent"].Value),
                Convert.ToDecimal(r.Cells["Discount"].Value)
            ));
        }

        var paid = decimal.TryParse(txtPaidAmount.Text, out var paidAmt) ? paidAmt : 0m;

        try
        {
            _purchaseService.AddPurchaseInvoice(Convert.ToInt32(cmbSupplier.SelectedValue), items, paid);
            MessageBox.Show("Purchase invoice saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dgvInvoiceLines.Rows.Clear();
            txtPaidAmount.Text = "0";
            RecalculateTotals();
            LoadPurchases();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryBuildCurrentInvoice(out InvoiceData invoice)
    {
        invoice = null!;
        if (dgvInvoiceLines.Rows.Count == 0 || cmbSupplier.SelectedValue == null)
            return false;

        var suppId = Convert.ToInt32(cmbSupplier.SelectedValue);
        var suppTbl = _supplierService.GetSupplierById(suppId);
        if (suppTbl.Rows.Count == 0) return false;
        var sr = suppTbl.Rows[0];

        invoice = new InvoiceData
        {
            InvoiceNumber   = $"PREVIEW-{DateTime.Now:HHmmss}",
            InvoiceDate     = DateTime.Now,
            TransactionType = "Purchase",
            PartyName       = sr["SupplierName"].ToString() ?? string.Empty,
            PartyAddress    = sr["Address"].ToString() ?? string.Empty,
            PartyMobile     = sr["Mobile"].ToString() ?? string.Empty,
            PartyGstNumber  = sr["GstNumber"].ToString() ?? string.Empty,
            InvoiceType     = cmbInvoiceType.SelectedItem?.ToString() ?? "A4"
        };

        foreach (DataGridViewRow r in dgvInvoiceLines.Rows)
        {
            if (r.IsNewRow) continue;
            invoice.Items.Add(new InvoiceItem
            {
                Description = r.Cells["ProductName"].Value?.ToString() ?? string.Empty,
                Quantity    = Convert.ToInt32(r.Cells["Quantity"].Value ?? 0),
                UnitPrice   = Convert.ToDecimal(r.Cells["PurchasePrice"].Value ?? 0m),
                GstPercent  = Convert.ToDecimal(r.Cells["GstPercent"].Value ?? 0m),
                Discount    = Convert.ToDecimal(r.Cells["Discount"].Value ?? 0m)
            });
        }
        return invoice.Items.Count > 0;
    }

    private bool TryGetSelectedPurchaseInvoice(out InvoiceData invoice)
    {
        invoice = null!;

        if (dgvPurchases.CurrentRow?.DataBoundItem is not DataRowView row)
            return false;

        var invoiceNumber = row.Row.Table.Columns.Contains("InvoiceNumber") && row["InvoiceNumber"] != DBNull.Value
            ? row["InvoiceNumber"].ToString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrEmpty(invoiceNumber)) return false;

        var supplierId = Convert.ToInt32(row["SupplierId"]);
        var supplierTable = _supplierService.GetSupplierById(supplierId);
        if (supplierTable.Rows.Count == 0) return false;
        var supplierRow = supplierTable.Rows[0];

        var itemsTable = _purchaseService.GetPurchaseInvoiceItems(invoiceNumber);

        invoice = new InvoiceData
        {
            InvoiceNumber   = invoiceNumber,
            InvoiceDate     = Convert.ToDateTime(row["PurchaseDate"]),
            TransactionType = "Purchase",
            PartyName       = supplierRow["SupplierName"].ToString() ?? string.Empty,
            PartyAddress    = supplierRow["Address"].ToString() ?? string.Empty,
            PartyMobile     = supplierRow["Mobile"].ToString() ?? string.Empty,
            PartyGstNumber  = supplierRow["GstNumber"].ToString() ?? string.Empty,
            InvoiceType     = cmbInvoiceType.SelectedItem?.ToString() ?? "A4"
        };

        if (itemsTable.Rows.Count > 0)
        {
            foreach (DataRow ir in itemsTable.Rows)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    Description = ir["ProductName"].ToString() ?? string.Empty,
                    Quantity    = Convert.ToInt32(ir["Quantity"]),
                    UnitPrice   = Convert.ToDecimal(ir["PurchasePrice"]),
                    GstPercent  = Convert.ToDecimal(ir["GstPercent"]),
                    Discount    = Convert.ToDecimal(ir["Discount"])
                });
            }
        }
        else
        {
            // fallback to single-row legacy data
            invoice.Items.Add(new InvoiceItem
            {
                Description = row["ProductName"].ToString() ?? string.Empty,
                Quantity    = Convert.ToInt32(row["Quantity"]),
                UnitPrice   = Convert.ToDecimal(row["PurchasePrice"]),
                GstPercent  = Convert.ToDecimal(row["GstPercent"]),
                Discount    = Convert.ToDecimal(row["Discount"])
            });
        }

        return invoice.Items.Count > 0;
    }

    private void btnPrintInvoice_Click(object sender, EventArgs e)
    {
        if (TryBuildCurrentInvoice(out var invoice) || TryGetSelectedPurchaseInvoice(out invoice))
            InvoicePrinter.ShowPrintPreview(invoice);
        else
            MessageBox.Show("Add items to the invoice or select a saved invoice from the grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void btnPrintToPrinter_Click(object sender, EventArgs e)
    {
        if (TryBuildCurrentInvoice(out var invoice) || TryGetSelectedPurchaseInvoice(out invoice))
            InvoicePrinter.PrintInvoice(invoice);
        else
            MessageBox.Show("Add items to the invoice or select a saved invoice from the grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
