using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.Forms;

public partial class ReportsForm : Form
{
    public ReportsForm()
    {
        InitializeComponent();
        LoadReport();
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        LoadReport();
    }

    private void LoadReport()
    {
        var reportType = cmbReportType.SelectedItem?.ToString();
        DataTable table;

        switch (reportType)
        {
            case "Sales Summary":
                table = SqlHelper.ExecuteDataTable(@"
                    SELECT s.SaleDate, c.CustomerName, p.ProductName, s.Quantity, s.SellingPrice, s.GstPercent, s.Discount, s.TotalAmount
                    FROM Sales s
                    INNER JOIN Customers c ON s.CustomerId = c.CustomerId
                    INNER JOIN Products p ON s.ProductId = p.ProductId
                    ORDER BY s.SaleDate DESC");
                break;
            case "Purchase Summary":
                table = SqlHelper.ExecuteDataTable(@"
                    SELECT p.PurchaseDate, s.SupplierName, pr.ProductName, p.Quantity, p.PurchasePrice, p.GstPercent, p.Discount, p.TotalAmount
                    FROM Purchases p
                    INNER JOIN Suppliers s ON p.SupplierId = s.SupplierId
                    INNER JOIN Products pr ON p.ProductId = pr.ProductId
                    ORDER BY p.PurchaseDate DESC");
                break;
            case "Low Stock":
                table = SqlHelper.ExecuteDataTable(@"
                    SELECT ProductName, ProductCode, CategoryName, CurrentStock, ReorderLevel
                    FROM Products
                    WHERE CurrentStock <= ReorderLevel
                    ORDER BY CurrentStock ASC");
                break;
            default:
                table = SqlHelper.ExecuteDataTable(@"
                    SELECT ProductName, ProductCode, CategoryName, CurrentStock, ReorderLevel, SellingPrice
                    FROM Products
                    ORDER BY ProductName");
                break;
        }

        dgvReport.DataSource = table;
    }
}
