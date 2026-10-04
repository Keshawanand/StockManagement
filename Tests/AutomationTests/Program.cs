using StockManagementApp.BusinessLogic;
using StockManagementApp.DataAccess;
using System.Data;
using System.Data.SqlClient;

Console.WriteLine("=== FRESH DATA FLOW TEST ===");

var purchSvc = new PurchaseService();
var salesSvc = new SalesService();
var adjSvc   = new StockAdjustmentService();

int passed = 0, failed = 0;

// Fetch IDs from seeded data
int prodTvId = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT TOP 1 ProductId FROM Products WHERE ProductCode='ELEC001'") ?? 0);
int prodShirtId = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT TOP 1 ProductId FROM Products WHERE ProductCode='CLTH001'") ?? 0);
int suppId  = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT TOP 1 SupplierId FROM Suppliers WHERE SupplierName='ABC Electronics'") ?? 0);
int custId  = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT TOP 1 CustomerId FROM Customers WHERE CustomerName='Rahul Sharma'") ?? 0);

Console.WriteLine($"TV product: {prodTvId}, Shirt: {prodShirtId}, Supplier: {suppId}, Customer: {custId}");

int stockTV    = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodTvId)) ?? 0);
int stockShirt = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodShirtId)) ?? 0);
Console.WriteLine($"Initial stock — TV: {stockTV}, Shirt: {stockShirt}");

// TEST 1: Purchase 5 TVs — stock should increase
try
{
    purchSvc.AddPurchaseInvoice(suppId, new List<(int,int,decimal,decimal,decimal)>
    {
        (prodTvId, 5, 15000m, 18m, 0m),
        (prodShirtId, 10, 500m, 5m, 0m)
    }, 80000m);

    int newTV    = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodTvId)) ?? 0);
    int newShirt = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodShirtId)) ?? 0);
    bool ok = (newTV == stockTV + 5) && (newShirt == stockShirt + 10);
    Console.WriteLine($"Test 1 (Purchase multi-item): TV {stockTV}→{newTV}, Shirt {stockShirt}→{newShirt} — {(ok ? "PASS" : "FAIL")}");
    if (ok) { passed++; stockTV = newTV; stockShirt = newShirt; } else failed++;
}
catch (Exception ex) { Console.WriteLine("Test 1 EXCEPTION: " + ex.Message); failed++; }

// TEST 2: Sell 3 TVs — stock should decrease
try
{
    salesSvc.AddSalesInvoice(custId, new List<(int,int,decimal,decimal,decimal)>
    {
        (prodTvId, 3, 19999m, 18m, 0m)
    }, 50000m);

    int newTV = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodTvId)) ?? 0);
    bool ok = newTV == stockTV - 3;
    Console.WriteLine($"Test 2 (Sale reduces stock): TV {stockTV}→{newTV} — {(ok ? "PASS" : "FAIL")}");
    if (ok) { passed++; stockTV = newTV; } else failed++;
}
catch (Exception ex) { Console.WriteLine("Test 2 EXCEPTION: " + ex.Message); failed++; }

// TEST 3: Sell 2 TVs + 5 Shirts in one invoice
try
{
    salesSvc.AddSalesInvoice(custId, new List<(int,int,decimal,decimal,decimal)>
    {
        (prodTvId,    2, 19999m, 18m, 500m),
        (prodShirtId, 5, 899m,    5m, 50m)
    }, 0m);

    int newTV    = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodTvId)) ?? 0);
    int newShirt = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodShirtId)) ?? 0);
    bool ok = (newTV == stockTV - 2) && (newShirt == stockShirt - 5);
    Console.WriteLine($"Test 3 (Multi-item sale): TV {stockTV}→{newTV}, Shirt {stockShirt}→{newShirt} — {(ok ? "PASS" : "FAIL")}");
    if (ok) { passed++; stockTV = newTV; stockShirt = newShirt; } else failed++;
}
catch (Exception ex) { Console.WriteLine("Test 3 EXCEPTION: " + ex.Message); failed++; }

// TEST 4: Stock adjustment increase
try
{
    adjSvc.AdjustStock(prodTvId, "Increase", 3, "Received damaged return");
    int newTV = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT CurrentStock FROM Products WHERE ProductId=@Id", new SqlParameter("@Id", prodTvId)) ?? 0);
    bool ok = newTV == stockTV + 3;
    Console.WriteLine($"Test 4 (Adj increase): TV {stockTV}→{newTV} — {(ok ? "PASS" : "FAIL")}");
    if (ok) { passed++; stockTV = newTV; } else failed++;
}
catch (Exception ex) { Console.WriteLine("Test 4 EXCEPTION: " + ex.Message); failed++; }

// TEST 5: Reconciliation
try
{
    var recon = new StockReconciliationService();
    var diffs = recon.GetDiscrepancies();
    bool ok = diffs.Rows.Count == 0;
    Console.WriteLine($"Test 5 (Reconciliation): {diffs.Rows.Count} discrepancies — {(ok ? "PASS" : "FAIL")}");
    if (ok) passed++; else failed++;
}
catch (Exception ex) { Console.WriteLine("Test 5 EXCEPTION: " + ex.Message); failed++; }

Console.WriteLine($"\nFinal stock — TV: {stockTV}, Shirt: {stockShirt}");
Console.WriteLine($"Results: {passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
