using StockManagementApp.BusinessLogic;

namespace StockManagementApp.Forms;

public partial class CategoryForm : Form
{
    private readonly CategoryService _service = new();
    private int _selectedCategoryId = 0;

    public CategoryForm()
    {
        InitializeComponent();
        LoadCategories();
    }

    private void LoadCategories()
    {
        dgvCategories.DataSource = _service.GetCategories();
    }

    private void dgvCategories_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvCategories.CurrentRow == null) return;
        _selectedCategoryId = Convert.ToInt32(dgvCategories.CurrentRow.Cells["CategoryId"].Value ?? 0);
        txtCategoryName.Text = dgvCategories.CurrentRow.Cells["CategoryName"].Value?.ToString();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
        {
            MessageBox.Show("Category name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.AddCategory(txtCategoryName.Text.Trim());
        LoadCategories();
        ClearForm();
        MessageBox.Show("Category added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (_selectedCategoryId <= 0)
        {
            MessageBox.Show("Select a category from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
        {
            MessageBox.Show("Category name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _service.UpdateCategory(_selectedCategoryId, txtCategoryName.Text.Trim());
        LoadCategories();
        ClearForm();
        MessageBox.Show("Category updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedCategoryId <= 0)
        {
            MessageBox.Show("Select a category from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Delete category '{txtCategoryName.Text}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            _service.DeleteCategory(_selectedCategoryId);
            LoadCategories();
            ClearForm();
            MessageBox.Show("Category deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClear_Click(object? sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedCategoryId = 0;
        txtCategoryName.Text = string.Empty;
        txtCategoryName.Focus();
    }
}
