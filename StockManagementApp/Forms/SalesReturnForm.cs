using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class SalesReturnForm : Form
{
    private readonly SalesReturnService _service = new();

    public SalesReturnForm()
    {
        InitializeComponent();
        LoadSalesReturns();
    }

    private void LoadSalesReturns()
    {
        dgvReturns.DataSource = _service.GetSalesReturns();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txtSaleId.Text.Trim(), out var saleId) || saleId <= 0)
        {
            MessageBox.Show("Please enter a valid sale ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtQuantity.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show("Please enter a valid return quantity greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _service.AddReturn(saleId, quantity);
            LoadSalesReturns();
            MessageBox.Show("Sales return recorded successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
