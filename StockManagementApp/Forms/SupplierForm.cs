using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class SupplierForm : Form
{
    private readonly SupplierService _service = new();
    private int _selectedSupplierId = 0;

    public SupplierForm()
    {
        InitializeComponent();
        LoadSuppliers();
    }

    private void LoadSuppliers()
    {
        dgvSuppliers.DataSource = _service.GetSuppliers();
    }

    private void dgvSuppliers_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvSuppliers.CurrentRow == null) return;
        _selectedSupplierId = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["SupplierId"].Value ?? 0);
        txtSupplierName.Text = dgvSuppliers.CurrentRow.Cells["SupplierName"].Value?.ToString();
        txtMobile.Text = dgvSuppliers.CurrentRow.Cells["Mobile"].Value?.ToString();
        txtEmail.Text = dgvSuppliers.CurrentRow.Cells["Email"].Value?.ToString();
        txtAddress.Text = dgvSuppliers.CurrentRow.Cells["Address"].Value?.ToString();
        txtGst.Text = dgvSuppliers.CurrentRow.Cells["GstNumber"].Value?.ToString();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
        {
            MessageBox.Show("Supplier name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.AddSupplier(txtSupplierName.Text.Trim(), txtMobile.Text.Trim(), txtEmail.Text.Trim(), txtAddress.Text.Trim(), txtGst.Text.Trim());
        LoadSuppliers();
        ClearForm();
        MessageBox.Show("Supplier added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (_selectedSupplierId <= 0)
        {
            MessageBox.Show("Select a supplier from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
        {
            MessageBox.Show("Supplier name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.UpdateSupplier(_selectedSupplierId, txtSupplierName.Text.Trim(), txtMobile.Text.Trim(), txtEmail.Text.Trim(), txtAddress.Text.Trim(), txtGst.Text.Trim());
        LoadSuppliers();
        ClearForm();
        MessageBox.Show("Supplier updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedSupplierId <= 0)
        {
            MessageBox.Show("Select a supplier from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Delete supplier '{txtSupplierName.Text}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            _service.DeleteSupplier(_selectedSupplierId);
            LoadSuppliers();
            ClearForm();
            MessageBox.Show("Supplier deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClear_Click(object? sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedSupplierId = 0;
        txtSupplierName.Text = string.Empty;
        txtMobile.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtAddress.Text = string.Empty;
        txtGst.Text = string.Empty;
        txtSupplierName.Focus();
    }
}
