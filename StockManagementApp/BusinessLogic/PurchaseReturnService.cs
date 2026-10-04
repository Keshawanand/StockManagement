using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class PurchaseReturnService
{
    public DataTable GetPurchaseReturns()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT r.PurchaseReturnId, p.PurchaseId, pr.ProductName, r.Quantity, r.ReturnDate, r.CreatedBy
            FROM PurchaseReturn r
            INNER JOIN Purchases p ON r.PurchaseId = p.PurchaseId
            INNER JOIN Products pr ON p.ProductId = pr.ProductId
            ORDER BY r.ReturnDate DESC");
    }

    public void AddReturn(int purchaseId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var purchaseInfo = SqlHelper.ExecuteDataTable(
                "SELECT ProductId, Quantity FROM Purchases WHERE PurchaseId = @PurchaseId",
                connection, transaction,
                new SqlParameter("@PurchaseId", purchaseId));

            if (purchaseInfo.Rows.Count == 0)
                throw new InvalidOperationException("Purchase transaction not found.");

            var productId = Convert.ToInt32(purchaseInfo.Rows[0]["ProductId"]);
            var originalQuantity = Convert.ToInt32(purchaseInfo.Rows[0]["Quantity"]);

            var returnedQuantity = Convert.ToInt32(SqlHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(Quantity), 0) FROM PurchaseReturn WHERE PurchaseId = @PurchaseId",
                connection, transaction,
                new SqlParameter("@PurchaseId", purchaseId)) ?? 0);

            if (quantity > originalQuantity - returnedQuantity)
                throw new InvalidOperationException("Return quantity exceeds the remaining purchased quantity.");

            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar(
                "SELECT CurrentStock FROM Products WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@ProductId", productId)) ?? 0);

            if (currentStock - quantity < 0)
                throw new InvalidOperationException("Stock cannot become negative from this purchase return.");

            var insertSql = @"INSERT INTO PurchaseReturn (PurchaseId, Quantity, ReturnDate, CreatedBy)
                               VALUES (@PurchaseId, @Quantity, @ReturnDate, @CreatedBy);
                               SELECT SCOPE_IDENTITY();";

            var newReturnIdObject = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@PurchaseId", purchaseId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (newReturnIdObject == null || !int.TryParse(newReturnIdObject.ToString(), out var returnId))
                throw new InvalidOperationException("Failed to save purchase return.");

            SqlHelper.ExecuteNonQuery(
                "UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock - quantity;
            StockHistoryService.RecordHistory(connection, transaction, productId, "PURCHASE_RETURN", returnId.ToString(), 0, quantity, currentStock, newStock, "Purchase return recorded", currentUser);
        });
    }

    public void AddReturnForInvoiceItem(int purchaseInvoiceItemId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var itemTable = SqlHelper.ExecuteDataTable("SELECT PurchaseInvoiceId, ProductId, Quantity FROM PurchaseInvoiceItems WHERE ItemId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceItemId));
            if (itemTable.Rows.Count == 0)
                throw new InvalidOperationException("Invoice item not found.");

            var purchaseInvoiceId = Convert.ToInt32(itemTable.Rows[0]["PurchaseInvoiceId"]);
            var productId = Convert.ToInt32(itemTable.Rows[0]["ProductId"]);
            var originalQty = Convert.ToInt32(itemTable.Rows[0]["Quantity"]);

            var returnedQty = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT ISNULL(SUM(Quantity),0) FROM PurchaseInvoiceReturns WHERE PurchaseInvoiceItemId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceItemId)) ?? 0);

            if (quantity > originalQty - returnedQty)
                throw new InvalidOperationException("Return quantity exceeds remaining quantity for this invoice item.");

            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
            if (currentStock - quantity < 0)
                throw new InvalidOperationException("Stock cannot become negative from this purchase return.");

            var insertSql = @"INSERT INTO PurchaseInvoiceReturns (PurchaseInvoiceItemId, Quantity, ReturnDate, CreatedBy)
                               VALUES (@ItemId, @Quantity, @ReturnDate, @CreatedBy);
                               SELECT SCOPE_IDENTITY();";

            var newReturnIdObj = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@ItemId", purchaseInvoiceItemId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (newReturnIdObj == null || !int.TryParse(newReturnIdObj.ToString(), out var returnId))
                throw new InvalidOperationException("Failed to save purchase invoice return.");

            SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId", connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock - quantity;
            // fetch invoice number for history reference
            var invoiceNumber = SqlHelper.ExecuteScalar("SELECT InvoiceNumber FROM PurchaseInvoices WHERE PurchaseInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceId))?.ToString() ?? string.Empty;
            StockHistoryService.RecordHistory(connection, transaction, productId, "PURCHASE_RETURN", invoiceNumber, 0, quantity, currentStock, newStock, "Purchase invoice return recorded", currentUser);
        });
    }
}
