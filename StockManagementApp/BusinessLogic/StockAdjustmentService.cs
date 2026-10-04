using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class StockAdjustmentService
{
    public DataTable GetAdjustments()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT a.AdjustmentId, p.ProductName, a.AdjustmentType, a.Quantity, a.Reason, a.AdjustmentDate, a.ChangedBy
            FROM StockAdjustment a
            INNER JOIN Products p ON a.ProductId = p.ProductId
            ORDER BY a.AdjustmentDate DESC");
    }

    public void AdjustStock(int productId, string adjustmentType, int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("Adjustment quantity must be greater than zero.", nameof(quantity));

        if (adjustmentType != "Increase" && adjustmentType != "Decrease")
            throw new ArgumentException("Adjustment type must be either 'Increase' or 'Decrease'.", nameof(adjustmentType));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar(
                "SELECT CurrentStock FROM Products WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@ProductId", productId)) ?? 0);

            var modifier = adjustmentType == "Increase" ? quantity : -quantity;
            var newStock = currentStock + modifier;
            if (newStock < 0)
                throw new InvalidOperationException("Stock cannot be adjusted below zero.");

            var insertSql = @"INSERT INTO StockAdjustment (ProductId, AdjustmentType, Quantity, Reason, AdjustmentDate, ChangedBy)
                               VALUES (@ProductId, @AdjustmentType, @Quantity, @Reason, @AdjustmentDate, @ChangedBy);
                               SELECT SCOPE_IDENTITY();";

            var adjustmentIdObject = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@AdjustmentType", adjustmentType),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@Reason", reason),
                new SqlParameter("@AdjustmentDate", DateTime.Now),
                new SqlParameter("@ChangedBy", currentUser));

            if (adjustmentIdObject == null || !int.TryParse(adjustmentIdObject.ToString(), out var adjustmentId))
                throw new InvalidOperationException("Failed to save stock adjustment.");

            SqlHelper.ExecuteNonQuery(
                "UPDATE Products SET CurrentStock = @NewStock WHERE ProductId = @ProductId",
                connection, transaction,
                new SqlParameter("@NewStock", newStock),
                new SqlParameter("@ProductId", productId));

            StockHistoryService.RecordHistory(connection, transaction, productId, "STOCK_ADJUSTMENT", adjustmentId.ToString(), adjustmentType == "Increase" ? quantity : 0, adjustmentType == "Decrease" ? quantity : 0, currentStock, newStock, reason, currentUser);
        });
    }
}
