using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class PurchaseReturnForm : Form
{
    private readonly PurchaseReturnService _service = new();

    public PurchaseReturnForm()
    {
        InitializeComponent();
        LoadPurchaseReturns();
    }

    private void LoadPurchaseReturns()
    {
        dgvReturns.DataSource = _service.GetPurchaseReturns();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txtPurchaseId.Text.Trim(), out var purchaseId) || purchaseId <= 0)
        {
            MessageBox.Show("Please enter a valid purchase ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtQuantity.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show("Please enter a valid return quantity greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _service.AddReturn(purchaseId, quantity);
            LoadPurchaseReturns();
            MessageBox.Show("Purchase return recorded successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
