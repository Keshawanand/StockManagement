using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class SupplierPaymentService
{
    public DataTable GetPayments()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT p.PaymentId, p.SupplierId, s.SupplierName, p.Amount, p.PaymentMethod, p.PaymentDate, p.Reference, p.CreatedBy
            FROM SupplierPayments p
            INNER JOIN Suppliers s ON p.SupplierId = s.SupplierId
            ORDER BY p.PaymentDate DESC");
    }

    public void AddPayment(int supplierId, decimal amount, string paymentMethod, string reference)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar(
                "SELECT CurrentBalance FROM Suppliers WHERE SupplierId = @SupplierId",
                connection, transaction,
                new SqlParameter("@SupplierId", supplierId)) ?? 0m);

            var insertSql = @"INSERT INTO SupplierPayments (SupplierId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                               VALUES (@SupplierId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                               SELECT SCOPE_IDENTITY();";

            var paymentIdObj = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@SupplierId", supplierId),
                new SqlParameter("@PaymentDate", DateTime.Now),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@PaymentMethod", paymentMethod ?? string.Empty),
                new SqlParameter("@Reference", reference ?? string.Empty),
                new SqlParameter("@CreatedBy", currentUser),
                new SqlParameter("@CreatedAt", DateTime.Now));

            if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                throw new InvalidOperationException("Failed to record supplier payment.");

            var newBalance = currentBalance - amount;

            SqlHelper.ExecuteNonQuery(
                "UPDATE Suppliers SET CurrentBalance = @NewBalance WHERE SupplierId = @SupplierId",
                connection, transaction,
                new SqlParameter("@NewBalance", newBalance),
                new SqlParameter("@SupplierId", supplierId));
        });
    }
}
