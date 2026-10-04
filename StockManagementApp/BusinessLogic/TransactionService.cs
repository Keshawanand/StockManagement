using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class TransactionService
{
    public DataTable GetDashboardSummary()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT 
                (SELECT COUNT(*) FROM Products) AS TotalProducts,
                (SELECT COUNT(*) FROM Customers) AS TotalCustomers,
                (SELECT COUNT(*) FROM Suppliers) AS TotalSuppliers,
                (SELECT COUNT(*) FROM Sales) AS TotalSales,
                (SELECT COUNT(*) FROM Purchases) AS TotalPurchases,
                (SELECT ISNULL(SUM(CurrentStock), 0) FROM Products) AS TotalStockQuantity,
                (SELECT COUNT(*) FROM Products WHERE CurrentStock <= ReorderLevel) AS LowStockProducts,
                (SELECT COUNT(*) FROM Products WHERE CurrentStock = 0) AS OutOfStockProducts");
    }

    public void SavePurchase(int supplierId, int productId, int quantity, decimal purchasePrice, decimal gstPercent, decimal discount)
    {
        var total = (quantity * purchasePrice) + ((quantity * purchasePrice) * gstPercent / 100) - discount;
        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Purchases (SupplierId, ProductId, Quantity, PurchasePrice, GstPercent, Discount, TotalAmount, PurchaseDate, CreatedAt, CreatedBy) VALUES (@SupplierId, @ProductId, @Quantity, @PurchasePrice, @GstPercent, @Discount, @TotalAmount, @PurchaseDate, @CreatedAt, @CreatedBy)",
            new SqlParameter("@SupplierId", supplierId),
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@PurchasePrice", purchasePrice),
            new SqlParameter("@GstPercent", gstPercent),
            new SqlParameter("@Discount", discount),
            new SqlParameter("@TotalAmount", total),
            new SqlParameter("@PurchaseDate", DateTime.Now),
            new SqlParameter("@CreatedAt", DateTime.Now),
            new SqlParameter("@CreatedBy", "admin"));

        SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId",
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@ProductId", productId));
    }

    public void SaveSale(int customerId, int productId, int quantity, decimal sellingPrice, decimal gstPercent, decimal discount)
    {
        var available = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", new SqlParameter("@ProductId", productId)) ?? 0);
        if (quantity > available)
        {
            throw new InvalidOperationException("Insufficient Stock");
        }

        var total = (quantity * sellingPrice) + ((quantity * sellingPrice) * gstPercent / 100) - discount;
        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Sales (CustomerId, ProductId, Quantity, SellingPrice, GstPercent, Discount, TotalAmount, SaleDate, CreatedAt, CreatedBy) VALUES (@CustomerId, @ProductId, @Quantity, @SellingPrice, @GstPercent, @Discount, @TotalAmount, @SaleDate, @CreatedAt, @CreatedBy)",
            new SqlParameter("@CustomerId", customerId),
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@SellingPrice", sellingPrice),
            new SqlParameter("@GstPercent", gstPercent),
            new SqlParameter("@Discount", discount),
            new SqlParameter("@TotalAmount", total),
            new SqlParameter("@SaleDate", DateTime.Now),
            new SqlParameter("@CreatedAt", DateTime.Now),
            new SqlParameter("@CreatedBy", "admin"));

        SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId",
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@ProductId", productId));
    }
}
