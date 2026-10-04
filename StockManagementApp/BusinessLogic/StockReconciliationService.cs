using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class StockReconciliationService
{
    public DataTable GetDiscrepancies()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT pr.ProductId, pr.ProductName, pr.CurrentStock,
                   ISNULL(sh.NewStock, 0) AS ExpectedStock,
                   pr.CurrentStock - ISNULL(sh.NewStock, 0) AS Difference
            FROM Products pr
            LEFT JOIN (
                SELECT ProductId, MAX(StockHistoryId) AS MaxId FROM StockHistory GROUP BY ProductId
            ) mh ON pr.ProductId = mh.ProductId
            LEFT JOIN StockHistory sh ON mh.MaxId = sh.StockHistoryId
            WHERE pr.CurrentStock <> ISNULL(sh.NewStock, 0)
            ORDER BY pr.ProductName");
    }

    public void FixDiscrepancy(int productId)
    {
        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var product = SqlHelper.ExecuteDataTable("SELECT ProductName, CurrentStock FROM Products WHERE ProductId = @Id", connection, transaction, new SqlParameter("@Id", productId));
            if (product.Rows.Count == 0) throw new InvalidOperationException("Product not found.");

            var currentStock = Convert.ToInt32(product.Rows[0]["CurrentStock"]);
            var expectedObj = SqlHelper.ExecuteScalar("SELECT TOP 1 NewStock FROM StockHistory WHERE ProductId = @Id ORDER BY StockHistoryId DESC", connection, transaction, new SqlParameter("@Id", productId));
            var expected = Convert.ToInt32(expectedObj ?? 0);
            if (currentStock == expected) return;

            var diff = expected - currentStock;
            // update product stock
            SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = @Stock WHERE ProductId = @Id", connection, transaction, new SqlParameter("@Stock", expected), new SqlParameter("@Id", productId));

            // record adjustment
            SqlHelper.ExecuteNonQuery(@"INSERT INTO StockAdjustment (ProductId, AdjustmentType, Quantity, Reason, AdjustmentDate, ChangedBy)
                                       VALUES (@ProductId, @Type, @Quantity, @Reason, @Date, @By)",
                connection, transaction,
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@Type", "Reconcile"),
                new SqlParameter("@Quantity", diff),
                new SqlParameter("@Reason", "Reconciled to stock history"),
                new SqlParameter("@Date", DateTime.Now),
                new SqlParameter("@By", currentUser));

            // record stock history
            var previous = currentStock;
            var newStock = expected;
            StockHistoryService.RecordHistory(connection, transaction, productId, "RECONCILE", "", 0, 0, previous, newStock, "Reconciled to history", currentUser);
        });
    }
}
