using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class SalesReturnService
{
    public DataTable GetSalesReturns()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT r.SalesReturnId, s.SaleId, p.ProductName, r.Quantity, r.ReturnDate, r.CreatedBy
            FROM SalesReturn r
            INNER JOIN Sales s ON r.SaleId = s.SaleId
            INNER JOIN Products p ON s.ProductId = p.ProductId
            ORDER BY r.ReturnDate DESC");
    }

    public void AddReturn(int saleId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var saleInfo = SqlHelper.ExecuteDataTable(
                "SELECT ProductId, Quantity FROM Sales WHERE SaleId = @SaleId",
                connection, transaction,
                new SqlParameter("@SaleId", saleId));

            if (saleInfo.Rows.Count == 0)
                throw new InvalidOperationException("Sales transaction not found.");

            var productId = Convert.ToInt32(saleInfo.Rows[0]["ProductId"]);
            var soldQuantity = Convert.ToInt32(saleInfo.Rows[0]["Quantity"]);

            var returnedQuantity = Convert.ToInt32(SqlHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(Quantity), 0) FROM SalesReturn WHERE SaleId = @SaleId",
                connection, transaction,
                new SqlParameter("@SaleId", saleId)) ?? 0);

            if (quantity > soldQuantity - returnedQuantity)
                throw new InvalidOperationException("Return quantity exceeds the remaining sold quantity.");

            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar(
                "SELECT CurrentStock FROM Products WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@ProductId", productId)) ?? 0);

            var insertSql = @"INSERT INTO SalesReturn (SaleId, Quantity, ReturnDate, CreatedBy)
                               VALUES (@SaleId, @Quantity, @ReturnDate, @CreatedBy);
                               SELECT SCOPE_IDENTITY();";

            var newReturnIdObject = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@SaleId", saleId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (newReturnIdObject == null || !int.TryParse(newReturnIdObject.ToString(), out var returnId))
                throw new InvalidOperationException("Failed to save sales return.");

            SqlHelper.ExecuteNonQuery(
                "UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock + quantity;
            StockHistoryService.RecordHistory(connection, transaction, productId, "SALES_RETURN", returnId.ToString(), quantity, 0, currentStock, newStock, "Sales return recorded", currentUser);
        });
    }

    public void AddReturnForInvoiceItem(int salesInvoiceItemId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var itemTable = SqlHelper.ExecuteDataTable("SELECT SalesInvoiceId, ProductId, Quantity FROM SalesInvoiceItems WHERE ItemId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceItemId));
            if (itemTable.Rows.Count == 0)
                throw new InvalidOperationException("Invoice item not found.");

            var salesInvoiceId = Convert.ToInt32(itemTable.Rows[0]["SalesInvoiceId"]);
            var productId = Convert.ToInt32(itemTable.Rows[0]["ProductId"]);
            var originalQty = Convert.ToInt32(itemTable.Rows[0]["Quantity"]);

            var returnedQty = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT ISNULL(SUM(Quantity),0) FROM SalesInvoiceReturns WHERE SalesInvoiceItemId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceItemId)) ?? 0);

            if (quantity > originalQty - returnedQty)
                throw new InvalidOperationException("Return quantity exceeds remaining quantity for this invoice item.");

            var insertSql = @"INSERT INTO SalesInvoiceReturns (SalesInvoiceItemId, Quantity, ReturnDate, CreatedBy)
                               VALUES (@ItemId, @Quantity, @ReturnDate, @CreatedBy);
                               SELECT SCOPE_IDENTITY();";

            var newReturnIdObj = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@ItemId", salesInvoiceItemId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (newReturnIdObj == null || !int.TryParse(newReturnIdObj.ToString(), out var returnId))
                throw new InvalidOperationException("Failed to save sales invoice return.");

            // update product stock (sales return increases stock)
            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
            SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId", connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock + quantity;
            var invoiceNumber = SqlHelper.ExecuteScalar("SELECT InvoiceNumber FROM SalesInvoices WHERE SalesInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceId))?.ToString() ?? string.Empty;
            StockHistoryService.RecordHistory(connection, transaction, productId, "SALE_RETURN", invoiceNumber, quantity, 0, currentStock, newStock, "Sales invoice return recorded", currentUser);
        });
    }
}
