using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public static class StockHistoryService
{
    public static void RecordHistory(SqlConnection connection, SqlTransaction transaction, int productId, string changeType, string referenceId, int quantityIn, int quantityOut, int previousStock, int newStock, string reason, string changedBy)
    {
        SqlHelper.ExecuteNonQuery(
            @"INSERT INTO StockHistory (ProductId, ChangeType, ReferenceId, Quantity, QuantityIn, QuantityOut, PreviousStock, NewStock, Reason, ChangeDate, ChangedBy)
              VALUES (@ProductId, @ChangeType, @ReferenceId, @Quantity, @QuantityIn, @QuantityOut, @PreviousStock, @NewStock, @Reason, @ChangeDate, @ChangedBy)",
            connection,
            transaction,
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@ChangeType", changeType),
            new SqlParameter("@ReferenceId", referenceId),
            new SqlParameter("@Quantity", quantityIn - quantityOut),
            new SqlParameter("@QuantityIn", quantityIn),
            new SqlParameter("@QuantityOut", quantityOut),
            new SqlParameter("@PreviousStock", previousStock),
            new SqlParameter("@NewStock", newStock),
            new SqlParameter("@Reason", reason),
            new SqlParameter("@ChangeDate", DateTime.Now),
            new SqlParameter("@ChangedBy", changedBy));
    }
}
