using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class CustomerForm : Form
{
    private readonly CustomerService _service = new();
    private int _selectedCustomerId = 0;

    public CustomerForm()
    {
        InitializeComponent();
        LoadCustomers();
    }

    private void LoadCustomers()
    {
        dgvCustomers.DataSource = _service.GetCustomers();
    }

    private void dgvCustomers_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvCustomers.CurrentRow == null) return;
        _selectedCustomerId = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerId"].Value ?? 0);
        txtCustomerName.Text = dgvCustomers.CurrentRow.Cells["CustomerName"].Value?.ToString();
        txtMobile.Text = dgvCustomers.CurrentRow.Cells["Mobile"].Value?.ToString();
        txtEmail.Text = dgvCustomers.CurrentRow.Cells["Email"].Value?.ToString();
        txtAddress.Text = dgvCustomers.CurrentRow.Cells["Address"].Value?.ToString();
        txtGst.Text = dgvCustomers.CurrentRow.Cells["GstNumber"].Value?.ToString();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            MessageBox.Show("Customer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.AddCustomer(txtCustomerName.Text.Trim(), txtMobile.Text.Trim(), txtEmail.Text.Trim(), txtAddress.Text.Trim(), txtGst.Text.Trim());
        LoadCustomers();
        ClearForm();
        MessageBox.Show("Customer added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (_selectedCustomerId <= 0)
        {
            MessageBox.Show("Select a customer from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            MessageBox.Show("Customer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.UpdateCustomer(_selectedCustomerId, txtCustomerName.Text.Trim(), txtMobile.Text.Trim(), txtEmail.Text.Trim(), txtAddress.Text.Trim(), txtGst.Text.Trim());
        LoadCustomers();
        ClearForm();
        MessageBox.Show("Customer updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedCustomerId <= 0)
        {
            MessageBox.Show("Select a customer from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Delete customer '{txtCustomerName.Text}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            _service.DeleteCustomer(_selectedCustomerId);
            LoadCustomers();
            ClearForm();
            MessageBox.Show("Customer deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClear_Click(object? sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedCustomerId = 0;
        txtCustomerName.Text = string.Empty;
        txtMobile.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtAddress.Text = string.Empty;
        txtGst.Text = string.Empty;
        txtCustomerName.Focus();
    }
}
