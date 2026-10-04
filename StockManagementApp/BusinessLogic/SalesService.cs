using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class SalesService
{
    public DataTable GetSales()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT s.SaleId, s.CustomerId, s.ProductId, s.InvoiceNumber, c.CustomerName, p.ProductName, s.Quantity, s.SellingPrice, s.GstPercent, s.Discount, s.TotalAmount, s.SaleDate
            FROM Sales s
            INNER JOIN Customers c ON s.CustomerId = c.CustomerId
            INNER JOIN Products p ON s.ProductId = p.ProductId
            ORDER BY s.SaleDate DESC");
    }

    public DataTable GetInvoiceItems(string invoiceNumber)
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT p.ProductName, i.Quantity, i.SellingPrice, i.GstPercent, i.Discount, i.LineTotal
            FROM SalesInvoiceItems i
            INNER JOIN SalesInvoices h ON i.SalesInvoiceId = h.SalesInvoiceId
            INNER JOIN Products p ON i.ProductId = p.ProductId
            WHERE h.InvoiceNumber = @InvoiceNumber",
            new SqlParameter("@InvoiceNumber", invoiceNumber));
    }

    public DataTable GetSalesInvoices()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT h.SalesInvoiceId, h.InvoiceNumber, c.CustomerName, h.InvoiceDate, h.TotalAmount, h.PaidAmount, h.DueAmount, h.IsCancelled
            FROM SalesInvoices h
            INNER JOIN Customers c ON h.CustomerId = c.CustomerId
            ORDER BY h.InvoiceDate DESC");
    }

    public void AddSale(int customerId, int productId, int quantity, decimal sellingPrice, decimal gstPercent, decimal discount)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (sellingPrice < 0)
            throw new ArgumentException("Selling price cannot be negative.", nameof(sellingPrice));
        if (gstPercent < 0)
            throw new ArgumentException("GST percent cannot be negative.", nameof(gstPercent));
        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative.", nameof(discount));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var available = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
            if (quantity > available)
                throw new InvalidOperationException("Insufficient stock available.");

            var currentStock = available;
            var total = (quantity * sellingPrice) + ((quantity * sellingPrice) * gstPercent / 100) - discount;

            // generate invoice number
            var invoiceNumber = $"SAL-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // insert sales invoice header
            var insertInvoiceSql = @"INSERT INTO SalesInvoices (InvoiceNumber, CustomerId, InvoiceDate, TotalAmount, PaidAmount, CreatedAt, CreatedBy)
                                     VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @TotalAmount, @PaidAmount, @CreatedAt, @CreatedBy);
                                     SELECT SCOPE_IDENTITY();";

            var invoiceIdObj = SqlHelper.ExecuteScalar(insertInvoiceSql, connection, transaction,
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@InvoiceDate", DateTime.Now),
                new SqlParameter("@TotalAmount", total),
                new SqlParameter("@PaidAmount", 0),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (invoiceIdObj == null || !int.TryParse(invoiceIdObj.ToString(), out var invoiceId))
                throw new InvalidOperationException("Failed to save sales invoice.");

            // audit
            AuditService.RecordAudit(connection, transaction, "Create", "SalesInvoice", invoiceNumber, $"Total:{total}", currentUser);

            // insert invoice item
            SqlHelper.ExecuteNonQuery(@"INSERT INTO SalesInvoiceItems (SalesInvoiceId, ProductId, Quantity, SellingPrice, GstPercent, Discount, LineTotal)
                                        VALUES (@SalesInvoiceId, @ProductId, @Quantity, @SellingPrice, @GstPercent, @Discount, @LineTotal)",
                connection, transaction,
                new SqlParameter("@SalesInvoiceId", invoiceId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@SellingPrice", sellingPrice),
                new SqlParameter("@GstPercent", gstPercent),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@LineTotal", total));

            // insert legacy sale row (for compatibility) with invoice number
            var insertSaleSql = @"INSERT INTO Sales (CustomerId, ProductId, InvoiceNumber, Quantity, SellingPrice, GstPercent, Discount, TotalAmount, SaleDate, CreatedAt, CreatedBy)
                                   VALUES (@CustomerId, @ProductId, @InvoiceNumber, @Quantity, @SellingPrice, @GstPercent, @Discount, @TotalAmount, @SaleDate, @CreatedAt, @CreatedBy);
                                   SELECT SCOPE_IDENTITY();";

            var saleIdObject = SqlHelper.ExecuteScalar(insertSaleSql, connection, transaction,
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@SellingPrice", sellingPrice),
                new SqlParameter("@GstPercent", gstPercent),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@TotalAmount", total),
                new SqlParameter("@SaleDate", DateTime.Now),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (saleIdObject == null || !int.TryParse(saleIdObject.ToString(), out var saleId))
                throw new InvalidOperationException("Failed to save sales transaction.");

            SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId", connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock - quantity;
            StockHistoryService.RecordHistory(connection, transaction, productId, "SALE", invoiceNumber, 0, quantity, currentStock, newStock, "Sale saved", currentUser);
        });
    }

    public void AddSalesInvoice(int customerId, List<(int ProductId, int Quantity, decimal SellingPrice, decimal GstPercent, decimal Discount)> items, decimal paidAmount)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Invoice must contain at least one item.", nameof(items));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            decimal invoiceTotal = 0m;
            foreach (var it in items)
            {
                var lineTotal = (it.Quantity * it.SellingPrice) + ((it.Quantity * it.SellingPrice) * it.GstPercent / 100) - it.Discount;
                invoiceTotal += lineTotal;
            }

            var invoiceNumber = $"SAL-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var insertInvoiceSql = @"INSERT INTO SalesInvoices (InvoiceNumber, CustomerId, InvoiceDate, TotalAmount, PaidAmount, CreatedAt, CreatedBy)
                                     VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @TotalAmount, @PaidAmount, @CreatedAt, @CreatedBy);
                                     SELECT SCOPE_IDENTITY();";

            var invoiceIdObj = SqlHelper.ExecuteScalar(insertInvoiceSql, connection, transaction,
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@InvoiceDate", DateTime.Now),
                new SqlParameter("@TotalAmount", invoiceTotal),
                new SqlParameter("@PaidAmount", paidAmount),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (invoiceIdObj == null || !int.TryParse(invoiceIdObj.ToString(), out var invoiceId))
                throw new InvalidOperationException("Failed to save sales invoice.");

            // process items
            foreach (var it in items)
            {
                var lineTotal = (it.Quantity * it.SellingPrice) + ((it.Quantity * it.SellingPrice) * it.GstPercent / 100) - it.Discount;

                SqlHelper.ExecuteNonQuery(@"INSERT INTO SalesInvoiceItems (SalesInvoiceId, ProductId, Quantity, SellingPrice, GstPercent, Discount, LineTotal)
                                            VALUES (@SalesInvoiceId, @ProductId, @Quantity, @SellingPrice, @GstPercent, @Discount, @LineTotal)",
                    connection, transaction,
                    new SqlParameter("@SalesInvoiceId", invoiceId),
                    new SqlParameter("@ProductId", it.ProductId),
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@SellingPrice", it.SellingPrice),
                    new SqlParameter("@GstPercent", it.GstPercent),
                    new SqlParameter("@Discount", it.Discount),
                    new SqlParameter("@LineTotal", lineTotal));

                // insert legacy sale row per item
                var saleIdObject = SqlHelper.ExecuteScalar(@"INSERT INTO Sales (CustomerId, ProductId, InvoiceNumber, Quantity, SellingPrice, GstPercent, Discount, TotalAmount, SaleDate, CreatedAt, CreatedBy)
                                   VALUES (@CustomerId, @ProductId, @InvoiceNumber, @Quantity, @SellingPrice, @GstPercent, @Discount, @TotalAmount, @SaleDate, @CreatedAt, @CreatedBy);
                                   SELECT SCOPE_IDENTITY();",
                    connection, transaction,
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@ProductId", it.ProductId),
                    new SqlParameter("@InvoiceNumber", invoiceNumber),
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@SellingPrice", it.SellingPrice),
                    new SqlParameter("@GstPercent", it.GstPercent),
                    new SqlParameter("@Discount", it.Discount),
                    new SqlParameter("@TotalAmount", lineTotal),
                    new SqlParameter("@SaleDate", DateTime.Now),
                    new SqlParameter("@CreatedAt", DateTime.Now),
                    new SqlParameter("@CreatedBy", currentUser));

                if (saleIdObject == null || !int.TryParse(saleIdObject.ToString(), out var saleId))
                    throw new InvalidOperationException("Failed to save sales transaction item.");

                // update product stock
                var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", it.ProductId)) ?? 0);
                if (it.Quantity > currentStock)
                    throw new InvalidOperationException($"Insufficient stock for product {it.ProductId}.");

                SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId", connection, transaction,
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@ProductId", it.ProductId));

                var newStock = currentStock - it.Quantity;
                StockHistoryService.RecordHistory(connection, transaction, it.ProductId, "SALE", invoiceNumber, 0, it.Quantity, currentStock, newStock, "Sales invoice saved", currentUser);
            }

            // record payment if any
            if (paidAmount > 0)
            {
                var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar(
                    "SELECT CurrentBalance FROM Customers WHERE CustomerId = @CustomerId", connection, transaction, new SqlParameter("@CustomerId", customerId)) ?? 0m);

                var insertPaymentSql = @"INSERT INTO CustomerPayments (CustomerId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                                          VALUES (@CustomerId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                                          SELECT SCOPE_IDENTITY();";

                var paymentIdObj = SqlHelper.ExecuteScalar(insertPaymentSql, connection, transaction,
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@PaymentDate", DateTime.Now),
                    new SqlParameter("@Amount", paidAmount),
                    new SqlParameter("@PaymentMethod", "Invoice Payment"),
                    new SqlParameter("@Reference", invoiceNumber),
                    new SqlParameter("@CreatedBy", currentUser),
                    new SqlParameter("@CreatedAt", DateTime.Now));

                if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                    throw new InvalidOperationException("Failed to record customer payment.");

                var newBalance = currentBalance - paidAmount;

                SqlHelper.ExecuteNonQuery(
                    "UPDATE Customers SET CurrentBalance = @NewBalance WHERE CustomerId = @CustomerId",
                    connection, transaction,
                    new SqlParameter("@NewBalance", newBalance),
                    new SqlParameter("@CustomerId", customerId));
            }
        });
    }

    public void CancelSalesInvoice(int salesInvoiceId)
    {
        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var invoiceRow = SqlHelper.ExecuteDataTable("SELECT InvoiceNumber, CustomerId, TotalAmount, PaidAmount, IsCancelled FROM SalesInvoices WHERE SalesInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceId));
            if (invoiceRow.Rows.Count == 0)
                throw new InvalidOperationException("Sales invoice not found.");

            var row = invoiceRow.Rows[0];
            if (Convert.ToBoolean(row["IsCancelled"]))
                throw new InvalidOperationException("Invoice is already cancelled.");

            var invoiceNumber = row["InvoiceNumber"].ToString() ?? string.Empty;
            var customerId = Convert.ToInt32(row["CustomerId"]);
            var paidAmount = Convert.ToDecimal(row["PaidAmount"]);

            var items = SqlHelper.ExecuteDataTable("SELECT ProductId, Quantity FROM SalesInvoiceItems WHERE SalesInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceId));

            // reverse stock
            foreach (DataRow it in items.Rows)
            {
                var productId = Convert.ToInt32(it["ProductId"]);
                var qty = Convert.ToInt32(it["Quantity"]);

                var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
                SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId", connection, transaction,
                    new SqlParameter("@Quantity", qty),
                    new SqlParameter("@ProductId", productId));

                var newStock = currentStock + qty;
                StockHistoryService.RecordHistory(connection, transaction, productId, "SALE_CANCEL", invoiceNumber, qty, 0, currentStock, newStock, "Sales invoice cancelled", currentUser);
            }

            // mark invoice cancelled
            SqlHelper.ExecuteNonQuery("UPDATE SalesInvoices SET IsCancelled = 1 WHERE SalesInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", salesInvoiceId));

            // reverse payment impact: create negative payment record and adjust customer balance
            if (paidAmount > 0)
            {
                var insertPaymentSql = @"INSERT INTO CustomerPayments (CustomerId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                                          VALUES (@CustomerId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                                          SELECT SCOPE_IDENTITY();";

                var paymentIdObj = SqlHelper.ExecuteScalar(insertPaymentSql, connection, transaction,
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@PaymentDate", DateTime.Now),
                    new SqlParameter("@Amount", -paidAmount),
                    new SqlParameter("@PaymentMethod", "Invoice Cancellation"),
                    new SqlParameter("@Reference", invoiceNumber),
                    new SqlParameter("@CreatedBy", currentUser),
                    new SqlParameter("@CreatedAt", DateTime.Now));

                if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                    throw new InvalidOperationException("Failed to record reversal payment.");

                var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar("SELECT CurrentBalance FROM Customers WHERE CustomerId = @CustomerId", connection, transaction, new SqlParameter("@CustomerId", customerId)) ?? 0m);
                var newBalance = currentBalance + paidAmount;

                SqlHelper.ExecuteNonQuery("UPDATE Customers SET CurrentBalance = @NewBalance WHERE CustomerId = @CustomerId", connection, transaction,
                    new SqlParameter("@NewBalance", newBalance),
                    new SqlParameter("@CustomerId", customerId));
            }

            AuditService.RecordAudit(connection, transaction, "Cancel", "SalesInvoice", invoiceNumber, $"Cancelled by {currentUser}", currentUser);
        });
    }
}
