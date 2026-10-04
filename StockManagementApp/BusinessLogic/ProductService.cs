using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class ProductService
{
    public DataTable GetProducts()
    {
        return SqlHelper.ExecuteDataTable("SELECT ProductId, ProductName, ProductCode, Barcode, CategoryName, Brand, Unit, PurchasePrice, SellingPrice, GstPercent, ReorderLevel, CurrentStock, Description FROM Products ORDER BY ProductName");
    }

    public DataTable SearchProducts(string keyword)
    {
        return SqlHelper.ExecuteDataTable(
            "SELECT ProductId, ProductName, ProductCode, Barcode, CategoryName, Brand, Unit, PurchasePrice, SellingPrice, GstPercent, ReorderLevel, CurrentStock, Description FROM Products WHERE ProductName LIKE @Keyword OR ProductCode LIKE @Keyword OR Barcode LIKE @Keyword",
            new SqlParameter("@Keyword", $"%{keyword}%"));
    }

    public DataTable GetProductById(int productId)
    {
        return SqlHelper.ExecuteDataTable("SELECT ProductId, ProductName, ProductCode, Barcode, CategoryName, Brand, Unit, PurchasePrice, SellingPrice, GstPercent, ReorderLevel, CurrentStock, Description FROM Products WHERE ProductId = @ProductId", new SqlParameter("@ProductId", productId));
    }

    public void AddProduct(string productName, string productCode, string barcode, string categoryName, string brand, string unit, decimal purchasePrice, decimal sellingPrice, decimal gstPercent, int reorderLevel, int currentStock, string description)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code is required.", nameof(productCode));
        if (purchasePrice < 0) throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));
        if (sellingPrice < 0) throw new ArgumentException("Selling price cannot be negative.", nameof(sellingPrice));
        if (gstPercent < 0) throw new ArgumentException("GST percent cannot be negative.", nameof(gstPercent));

        // ensure category exists
        var cat = SqlHelper.ExecuteDataTable("SELECT CategoryId FROM Categories WHERE CategoryName = @Name", new SqlParameter("@Name", categoryName));
        if (cat.Rows.Count == 0)
            throw new InvalidOperationException($"Category '{categoryName}' does not exist. Create the category first.");

        // ensure unique product code
        var existing = SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM Products WHERE ProductCode = @Code", new SqlParameter("@Code", productCode));
        if (Convert.ToInt32(existing ?? 0) > 0)
            throw new InvalidOperationException("Product code must be unique.");

        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Products (ProductName, ProductCode, Barcode, CategoryName, Brand, Unit, PurchasePrice, SellingPrice, GstPercent, ReorderLevel, CurrentStock, Description, CreatedAt) VALUES (@ProductName, @ProductCode, @Barcode, @CategoryName, @Brand, @Unit, @PurchasePrice, @SellingPrice, @GstPercent, @ReorderLevel, @CurrentStock, @Description, @CreatedAt)",
            new SqlParameter("@ProductName", productName),
            new SqlParameter("@ProductCode", productCode),
            new SqlParameter("@Barcode", barcode),
            new SqlParameter("@CategoryName", categoryName),
            new SqlParameter("@Brand", brand),
            new SqlParameter("@Unit", unit),
            new SqlParameter("@PurchasePrice", purchasePrice),
            new SqlParameter("@SellingPrice", sellingPrice),
            new SqlParameter("@GstPercent", gstPercent),
            new SqlParameter("@ReorderLevel", reorderLevel),
            new SqlParameter("@CurrentStock", currentStock),
            new SqlParameter("@Description", description),
            new SqlParameter("@CreatedAt", DateTime.Now));
    }

    public void UpdateProduct(int productId, string productName, string productCode, string barcode, string categoryName, string brand, string unit, decimal purchasePrice, decimal sellingPrice, decimal gstPercent, int reorderLevel, int currentStock, string description)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code is required.", nameof(productCode));
        if (purchasePrice < 0) throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));
        if (sellingPrice < 0) throw new ArgumentException("Selling price cannot be negative.", nameof(sellingPrice));
        if (gstPercent < 0) throw new ArgumentException("GST percent cannot be negative.", nameof(gstPercent));

        var cat = SqlHelper.ExecuteDataTable("SELECT CategoryId FROM Categories WHERE CategoryName = @Name", new SqlParameter("@Name", categoryName));
        if (cat.Rows.Count == 0)
            throw new InvalidOperationException($"Category '{categoryName}' does not exist. Create the category first.");

        var existing = SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM Products WHERE ProductCode = @Code AND ProductId <> @ProductId", new SqlParameter("@Code", productCode), new SqlParameter("@ProductId", productId));
        if (Convert.ToInt32(existing ?? 0) > 0)
            throw new InvalidOperationException("Product code must be unique.");

        SqlHelper.ExecuteNonQuery(
            "UPDATE Products SET ProductName = @ProductName, ProductCode = @ProductCode, Barcode = @Barcode, CategoryName = @CategoryName, Brand = @Brand, Unit = @Unit, PurchasePrice = @PurchasePrice, SellingPrice = @SellingPrice, GstPercent = @GstPercent, ReorderLevel = @ReorderLevel, CurrentStock = @CurrentStock, Description = @Description WHERE ProductId = @ProductId",
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@ProductName", productName),
            new SqlParameter("@ProductCode", productCode),
            new SqlParameter("@Barcode", barcode),
            new SqlParameter("@CategoryName", categoryName),
            new SqlParameter("@Brand", brand),
            new SqlParameter("@Unit", unit),
            new SqlParameter("@PurchasePrice", purchasePrice),
            new SqlParameter("@SellingPrice", sellingPrice),
            new SqlParameter("@GstPercent", gstPercent),
            new SqlParameter("@ReorderLevel", reorderLevel),
            new SqlParameter("@CurrentStock", currentStock),
            new SqlParameter("@Description", description));
    }

    public void DeleteProduct(int productId)
    {
        var salesCount = Convert.ToInt32(SqlHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Sales WHERE ProductId = @ProductId", new SqlParameter("@ProductId", productId)) ?? 0);
        var purchaseCount = Convert.ToInt32(SqlHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Purchases WHERE ProductId = @ProductId", new SqlParameter("@ProductId", productId)) ?? 0);

        if (salesCount > 0 || purchaseCount > 0)
            throw new InvalidOperationException(
                $"Cannot delete this product — it has {salesCount} sale(s) and {purchaseCount} purchase(s) linked to it. Deactivate it instead.");

        SqlHelper.ExecuteNonQuery("DELETE FROM Products WHERE ProductId = @ProductId", new SqlParameter("@ProductId", productId));
    }
}
