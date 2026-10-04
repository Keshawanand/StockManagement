using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class PurchaseService
{
    public DataTable GetPurchaseInvoiceItems(string invoiceNumber)
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT p.ProductName, i.Quantity, i.PurchasePrice, i.GstPercent, i.Discount, i.LineTotal
            FROM PurchaseInvoiceItems i
            INNER JOIN PurchaseInvoices h ON i.PurchaseInvoiceId = h.PurchaseInvoiceId
            INNER JOIN Products p ON i.ProductId = p.ProductId
            WHERE h.InvoiceNumber = @InvoiceNumber",
            new SqlParameter("@InvoiceNumber", invoiceNumber));
    }

    public DataTable GetPurchases()
    {
        return SqlHelper.ExecuteDataTable(@"
            SELECT p.PurchaseId, p.SupplierId, p.ProductId, p.InvoiceNumber, s.SupplierName, pr.ProductName, p.Quantity, p.PurchasePrice, p.GstPercent, p.Discount, p.TotalAmount, p.PurchaseDate
            FROM Purchases p
            INNER JOIN Suppliers s ON p.SupplierId = s.SupplierId
            INNER JOIN Products pr ON p.ProductId = pr.ProductId
            ORDER BY p.PurchaseDate DESC");
    }

    public void AddPurchase(int supplierId, int productId, int quantity, decimal purchasePrice, decimal gstPercent, decimal discount)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (purchasePrice < 0)
            throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));
        if (gstPercent < 0)
            throw new ArgumentException("GST percent cannot be negative.", nameof(gstPercent));
        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative.", nameof(discount));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
            var total = (quantity * purchasePrice) + ((quantity * purchasePrice) * gstPercent / 100) - discount;

            // generate persistent invoice number
            var invoiceNumber = $"PUR-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // insert invoice header
            var insertInvoiceSql = @"INSERT INTO PurchaseInvoices (InvoiceNumber, SupplierId, InvoiceDate, TotalAmount, PaidAmount, CreatedAt, CreatedBy)
                                     VALUES (@InvoiceNumber, @SupplierId, @InvoiceDate, @TotalAmount, @PaidAmount, @CreatedAt, @CreatedBy);
                                     SELECT SCOPE_IDENTITY();";

            var invoiceIdObj = SqlHelper.ExecuteScalar(insertInvoiceSql, connection, transaction,
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@SupplierId", supplierId),
                new SqlParameter("@InvoiceDate", DateTime.Now),
                new SqlParameter("@TotalAmount", total),
                new SqlParameter("@PaidAmount", 0),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (invoiceIdObj == null || !int.TryParse(invoiceIdObj.ToString(), out var invoiceId))
                throw new InvalidOperationException("Failed to save purchase invoice.");

            // audit
            AuditService.RecordAudit(connection, transaction, "Create", "PurchaseInvoice", invoiceNumber, $"Total:{total}", currentUser);

            // insert invoice item
            SqlHelper.ExecuteNonQuery(@"INSERT INTO PurchaseInvoiceItems (PurchaseInvoiceId, ProductId, Quantity, PurchasePrice, GstPercent, Discount, LineTotal)
                                        VALUES (@PurchaseInvoiceId, @ProductId, @Quantity, @PurchasePrice, @GstPercent, @Discount, @LineTotal)",
                connection, transaction,
                new SqlParameter("@PurchaseInvoiceId", invoiceId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@PurchasePrice", purchasePrice),
                new SqlParameter("@GstPercent", gstPercent),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@LineTotal", total));

            // insert legacy purchase row (for compatibility) with invoice number
            var insertPurchaseSql = @"INSERT INTO Purchases (SupplierId, ProductId, InvoiceNumber, Quantity, PurchasePrice, GstPercent, Discount, TotalAmount, PurchaseDate, CreatedAt, CreatedBy)
                                       VALUES (@SupplierId, @ProductId, @InvoiceNumber, @Quantity, @PurchasePrice, @GstPercent, @Discount, @TotalAmount, @PurchaseDate, @CreatedAt, @CreatedBy);
                                       SELECT SCOPE_IDENTITY();";

            var purchaseIdObject = SqlHelper.ExecuteScalar(insertPurchaseSql, connection, transaction,
                new SqlParameter("@SupplierId", supplierId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@PurchasePrice", purchasePrice),
                new SqlParameter("@GstPercent", gstPercent),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@TotalAmount", total),
                new SqlParameter("@PurchaseDate", DateTime.Now),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (purchaseIdObject == null || !int.TryParse(purchaseIdObject.ToString(), out var purchaseId))
                throw new InvalidOperationException("Failed to save purchase transaction.");

            SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId", connection, transaction,
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@ProductId", productId));

            var newStock = currentStock + quantity;
            StockHistoryService.RecordHistory(connection, transaction, productId, "PURCHASE", invoiceNumber, quantity, 0, currentStock, newStock, "Purchase saved", currentUser);
        });
    }

    public void AddPurchaseInvoice(int supplierId, List<(int ProductId, int Quantity, decimal PurchasePrice, decimal GstPercent, decimal Discount)> items, decimal paidAmount)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Invoice must contain at least one item.", nameof(items));

        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            decimal invoiceTotal = 0m;
            foreach (var it in items)
            {
                var lineTotal = (it.Quantity * it.PurchasePrice) + ((it.Quantity * it.PurchasePrice) * it.GstPercent / 100) - it.Discount;
                invoiceTotal += lineTotal;
            }

            var invoiceNumber = $"PUR-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var insertInvoiceSql = @"INSERT INTO PurchaseInvoices (InvoiceNumber, SupplierId, InvoiceDate, TotalAmount, PaidAmount, CreatedAt, CreatedBy)
                                     VALUES (@InvoiceNumber, @SupplierId, @InvoiceDate, @TotalAmount, @PaidAmount, @CreatedAt, @CreatedBy);
                                     SELECT SCOPE_IDENTITY();";

            var invoiceIdObj = SqlHelper.ExecuteScalar(insertInvoiceSql, connection, transaction,
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@SupplierId", supplierId),
                new SqlParameter("@InvoiceDate", DateTime.Now),
                new SqlParameter("@TotalAmount", invoiceTotal),
                new SqlParameter("@PaidAmount", paidAmount),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@CreatedBy", currentUser));

            if (invoiceIdObj == null || !int.TryParse(invoiceIdObj.ToString(), out var invoiceId))
                throw new InvalidOperationException("Failed to save purchase invoice.");

            // process items
            foreach (var it in items)
            {
                var lineTotal = (it.Quantity * it.PurchasePrice) + ((it.Quantity * it.PurchasePrice) * it.GstPercent / 100) - it.Discount;

                SqlHelper.ExecuteNonQuery(@"INSERT INTO PurchaseInvoiceItems (PurchaseInvoiceId, ProductId, Quantity, PurchasePrice, GstPercent, Discount, LineTotal)
                                            VALUES (@PurchaseInvoiceId, @ProductId, @Quantity, @PurchasePrice, @GstPercent, @Discount, @LineTotal)",
                    connection, transaction,
                    new SqlParameter("@PurchaseInvoiceId", invoiceId),
                    new SqlParameter("@ProductId", it.ProductId),
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@PurchasePrice", it.PurchasePrice),
                    new SqlParameter("@GstPercent", it.GstPercent),
                    new SqlParameter("@Discount", it.Discount),
                    new SqlParameter("@LineTotal", lineTotal));

                // insert legacy purchase row per item
                var purchaseIdObject = SqlHelper.ExecuteScalar(@"INSERT INTO Purchases (SupplierId, ProductId, InvoiceNumber, Quantity, PurchasePrice, GstPercent, Discount, TotalAmount, PurchaseDate, CreatedAt, CreatedBy)
                                       VALUES (@SupplierId, @ProductId, @InvoiceNumber, @Quantity, @PurchasePrice, @GstPercent, @Discount, @TotalAmount, @PurchaseDate, @CreatedAt, @CreatedBy);
                                       SELECT SCOPE_IDENTITY();",
                    connection, transaction,
                    new SqlParameter("@SupplierId", supplierId),
                    new SqlParameter("@ProductId", it.ProductId),
                    new SqlParameter("@InvoiceNumber", invoiceNumber),
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@PurchasePrice", it.PurchasePrice),
                    new SqlParameter("@GstPercent", it.GstPercent),
                    new SqlParameter("@Discount", it.Discount),
                    new SqlParameter("@TotalAmount", lineTotal),
                    new SqlParameter("@PurchaseDate", DateTime.Now),
                    new SqlParameter("@CreatedAt", DateTime.Now),
                    new SqlParameter("@CreatedBy", currentUser));

                if (purchaseIdObject == null || !int.TryParse(purchaseIdObject.ToString(), out var purchaseId))
                    throw new InvalidOperationException("Failed to save purchase transaction item.");

                // update product stock
                var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", it.ProductId)) ?? 0);
                SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock + @Quantity WHERE ProductId = @ProductId", connection, transaction,
                    new SqlParameter("@Quantity", it.Quantity),
                    new SqlParameter("@ProductId", it.ProductId));

                var newStock = currentStock + it.Quantity;
                StockHistoryService.RecordHistory(connection, transaction, it.ProductId, "PURCHASE", invoiceNumber, it.Quantity, 0, currentStock, newStock, "Purchase invoice saved", currentUser);
            }

            // record payment if any
            if (paidAmount > 0)
            {
                var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar(
                    "SELECT CurrentBalance FROM Suppliers WHERE SupplierId = @SupplierId", connection, transaction, new SqlParameter("@SupplierId", supplierId)) ?? 0m);

                var insertPaymentSql = @"INSERT INTO SupplierPayments (SupplierId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                                          VALUES (@SupplierId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                                          SELECT SCOPE_IDENTITY();";

                var paymentIdObj = SqlHelper.ExecuteScalar(insertPaymentSql, connection, transaction,
                    new SqlParameter("@SupplierId", supplierId),
                    new SqlParameter("@PaymentDate", DateTime.Now),
                    new SqlParameter("@Amount", paidAmount),
                    new SqlParameter("@PaymentMethod", "Invoice Payment"),
                    new SqlParameter("@Reference", invoiceNumber),
                    new SqlParameter("@CreatedBy", currentUser),
                    new SqlParameter("@CreatedAt", DateTime.Now));

                if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                    throw new InvalidOperationException("Failed to record supplier payment.");

                var newBalance = currentBalance - paidAmount;

                SqlHelper.ExecuteNonQuery(
                    "UPDATE Suppliers SET CurrentBalance = @NewBalance WHERE SupplierId = @SupplierId",
                    connection, transaction,
                    new SqlParameter("@NewBalance", newBalance),
                    new SqlParameter("@SupplierId", supplierId));
            }
        });
    }

    public void CancelPurchaseInvoice(int purchaseInvoiceId)
    {
        var currentUser = WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;

        SqlHelper.ExecuteTransaction((connection, transaction) =>
        {
            var invoiceRow = SqlHelper.ExecuteDataTable("SELECT InvoiceNumber, SupplierId, TotalAmount, PaidAmount, IsCancelled FROM PurchaseInvoices WHERE PurchaseInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceId));
            if (invoiceRow.Rows.Count == 0)
                throw new InvalidOperationException("Purchase invoice not found.");

            var row = invoiceRow.Rows[0];
            if (Convert.ToBoolean(row["IsCancelled"]))
                throw new InvalidOperationException("Invoice is already cancelled.");

            var invoiceNumber = row["InvoiceNumber"].ToString() ?? string.Empty;
            var supplierId = Convert.ToInt32(row["SupplierId"]);
            var paidAmount = Convert.ToDecimal(row["PaidAmount"]);

            var items = SqlHelper.ExecuteDataTable("SELECT ProductId, Quantity FROM PurchaseInvoiceItems WHERE PurchaseInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceId));

            // ensure we can safely reverse stock (no negative resulting stock)
            foreach (DataRow it in items.Rows)
            {
                var productId = Convert.ToInt32(it["ProductId"]);
                var qty = Convert.ToInt32(it["Quantity"]);
                var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
                if (currentStock - qty < 0)
                    throw new InvalidOperationException($"Cannot cancel invoice: product {productId} would have negative stock.");
            }

            // reverse stock and record history
            foreach (DataRow it in items.Rows)
            {
                var productId = Convert.ToInt32(it["ProductId"]);
                var qty = Convert.ToInt32(it["Quantity"]);
                var currentStock = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId = @ProductId", connection, transaction, new SqlParameter("@ProductId", productId)) ?? 0);
                SqlHelper.ExecuteNonQuery("UPDATE Products SET CurrentStock = CurrentStock - @Quantity WHERE ProductId = @ProductId", connection, transaction,
                    new SqlParameter("@Quantity", qty),
                    new SqlParameter("@ProductId", productId));

                var newStock = currentStock - qty;
                StockHistoryService.RecordHistory(connection, transaction, productId, "PURCHASE_CANCEL", invoiceNumber, 0, qty, currentStock, newStock, "Purchase invoice cancelled", currentUser);
            }

            // mark invoice cancelled
            SqlHelper.ExecuteNonQuery("UPDATE PurchaseInvoices SET IsCancelled = 1 WHERE PurchaseInvoiceId = @Id", connection, transaction, new SqlParameter("@Id", purchaseInvoiceId));

            // reverse payment impact: create negative payment record and adjust supplier balance
            if (paidAmount > 0)
            {
                var insertPaymentSql = @"INSERT INTO SupplierPayments (SupplierId, PaymentDate, Amount, PaymentMethod, Reference, CreatedBy, CreatedAt)
                                          VALUES (@SupplierId, @PaymentDate, @Amount, @PaymentMethod, @Reference, @CreatedBy, @CreatedAt);
                                          SELECT SCOPE_IDENTITY();";

                var paymentIdObj = SqlHelper.ExecuteScalar(insertPaymentSql, connection, transaction,
                    new SqlParameter("@SupplierId", supplierId),
                    new SqlParameter("@PaymentDate", DateTime.Now),
                    new SqlParameter("@Amount", -paidAmount),
                    new SqlParameter("@PaymentMethod", "Invoice Cancellation"),
                    new SqlParameter("@Reference", invoiceNumber),
                    new SqlParameter("@CreatedBy", currentUser),
                    new SqlParameter("@CreatedAt", DateTime.Now));

                if (paymentIdObj == null || !int.TryParse(paymentIdObj.ToString(), out var paymentId))
                    throw new InvalidOperationException("Failed to record reversal payment.");

                var currentBalance = Convert.ToDecimal(SqlHelper.ExecuteScalar("SELECT CurrentBalance FROM Suppliers WHERE SupplierId = @SupplierId", connection, transaction, new SqlParameter("@SupplierId", supplierId)) ?? 0m);
                var newBalance = currentBalance + paidAmount;

                SqlHelper.ExecuteNonQuery("UPDATE Suppliers SET CurrentBalance = @NewBalance WHERE SupplierId = @SupplierId", connection, transaction,
                    new SqlParameter("@NewBalance", newBalance),
                    new SqlParameter("@SupplierId", supplierId));
            }

            AuditService.RecordAudit(connection, transaction, "Cancel", "PurchaseInvoice", invoiceNumber, $"Cancelled by {currentUser}", currentUser);
        });
    }
}
