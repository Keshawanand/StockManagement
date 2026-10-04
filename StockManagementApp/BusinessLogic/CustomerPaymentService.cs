using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class CustomerPaymentService
{
    public DataTable GetPayments()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT p.PaymentId, p.CustomerId, c.CustomerName, p.Amount, p.PaymentMethod, p.PaymentDate, p.Reference, p.CreatedBy
            FROM CustomerPayments p
            INNER JOIN Customers c ON p.CustomerId = c.CustomerId
            ORDER BY p.PaymentDate DESC");
    }

    public void AddPayment(int customerId, decimal amount, string paymentMethod, string reference)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar(
                "SELECT CurrentBalance FROM Customers WHERE CustomerId = @CustomerId",
                connection, transaction,
                new SqlParameter("@CustomerId", customerId)) ?? 0m);

            var insertSql = @"INSERT INTO CustomerPayments (CustomerId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                               VALUES (@CustomerId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                               SELECT SCOPE_IDENTITY();";

            var paymentIdObj = SqlHelper.ExecuteScalar(insertSql, connection, transaction,
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@PaymentDate", DateTime.Now),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@PaymentMethod", paymentMethod ?? string.Empty),
                new SqlParameter("@Reference", reference ?? string.Empty),
                new SqlParameter("@CreatedBy", currentUser),
                new SqlParameter("@CreatedAt", DateTime.Now));

            if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                throw new InvalidOperationException("Failed to record customer payment.");

            var newBalance = currentBalance - amount;

            SqlHelper.ExecuteNonQuery(
                "UPDATE Customers SET CurrentBalance = @NewBalance WHERE CustomerId = @CustomerId",
                connection, transaction,
                new SqlParameter("@NewBalance", newBalance),
                new SqlParameter("@CustomerId", customerId));
        });
    }
}
