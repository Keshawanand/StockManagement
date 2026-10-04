using System.Data;
using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class StockAdjustmentForm : Form
{
    private readonly StockAdjustmentService _service = new();
    private readonly ProductService _productService = new();

    public StockAdjustmentForm()
    {
        InitializeComponent();
        LoadProducts();
        LoadAdjustments();
    }

    private void LoadProducts()
    {
        var products = _productService.GetProducts();
        cmbProduct.DisplayMember = "ProductName";
        cmbProduct.ValueMember = "ProductId";
        cmbProduct.DataSource = products;
    }

    private void LoadAdjustments()
    {
        dgvAdjustments.DataSource = _service.GetAdjustments();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (cmbProduct.SelectedValue == null)
        {
            MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cmbAdjustmentType.SelectedItem == null)
        {
            MessageBox.Show("Please select an adjustment type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtQuantity.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show("Please enter a valid adjustment quantity greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _service.AdjustStock(
                Convert.ToInt32(cmbProduct.SelectedValue),
                cmbAdjustmentType.SelectedItem.ToString() ?? "Increase",
                quantity,
                txtReason.Text.Trim());

            LoadAdjustments();
            MessageBox.Show("Stock adjustment saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtQuantity.Text = string.Empty;
            txtReason.Text = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
