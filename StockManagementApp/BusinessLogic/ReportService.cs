using System.Data;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class ReportService
{
    public DataTable GetCustomerBalances()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT CustomerId, CustomerName, CurrentBalance FROM Customers ORDER BY CustomerName");
    }

    public DataTable GetSupplierBalances()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT SupplierId, SupplierName, CurrentBalance FROM Suppliers ORDER BY SupplierName");
    }

    public DataTable GetStockSummary()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT ProductId, ProductName, ProductCode, CurrentStock, ReorderLevel FROM Products ORDER BY ProductName");
    }
}
