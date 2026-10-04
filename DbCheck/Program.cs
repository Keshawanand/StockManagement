using System.Data.SqlClient;

string conn = Environment.GetEnvironmentVariable("STOCK_DB_SERVER") ?? "localhost";
string db = Environment.GetEnvironmentVariable("STOCK_DB_NAME") ?? "StockManagementDb";
var cs = $"Server={conn};Database={db};Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=True;";

Console.WriteLine($"Using connection string: Server={conn};Database={db};Integrated Security=SSPI;TrustServerCertificate=true;");

try
{
    using var connection = new SqlConnection(cs);
    connection.Open();
    Console.WriteLine("Connected to database successfully.");

    string[] requiredTables = new[] {"Users","Categories","Suppliers","Customers","Products","Purchases","Sales","StockHistory","StockAdjustment","SalesReturn","PurchaseReturn","PurchaseInvoices","PurchaseInvoiceItems","SalesInvoices","SalesInvoiceItems","CustomerPayments","SupplierPayments","AuditTrail"};

    foreach (var t in requiredTables)
    {
        var cmd = new SqlCommand($"SELECT CASE WHEN OBJECT_ID(@t, 'U') IS NOT NULL THEN 1 ELSE 0 END", connection);
        cmd.Parameters.AddWithValue("@t", t);
        var exists = (int)cmd.ExecuteScalar();
        Console.WriteLine($"Table {t}: {(exists==1?"FOUND":"MISSING")}");
    }

        // Ensure legacy Purchases and Sales have InvoiceNumber column
        var checkCols = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Purchases'", connection);
        var purchCols = new List<string>();
        using (var r = checkCols.ExecuteReader()) { while (r.Read()) purchCols.Add(r.GetString(0)); }
        if (!purchCols.Contains("InvoiceNumber"))
        {
            try { using var c = new SqlCommand("ALTER TABLE Purchases ADD InvoiceNumber NVARCHAR(100) NULL", connection); c.ExecuteNonQuery(); Console.WriteLine("Added InvoiceNumber to Purchases"); }
            catch (Exception ex) { Console.WriteLine("Could not add InvoiceNumber to Purchases: " + ex.Message); }
        }

        checkCols = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sales'", connection);
        var salesCols = new List<string>();
        using (var r = checkCols.ExecuteReader()) { while (r.Read()) salesCols.Add(r.GetString(0)); }
        if (!salesCols.Contains("InvoiceNumber"))
        {
            try { using var c = new SqlCommand("ALTER TABLE Sales ADD InvoiceNumber NVARCHAR(100) NULL", connection); c.ExecuteNonQuery(); Console.WriteLine("Added InvoiceNumber to Sales"); }
            catch (Exception ex) { Console.WriteLine("Could not add InvoiceNumber to Sales: " + ex.Message); }
        }

        // Ensure Customers and Suppliers have CurrentBalance column
        var custColsCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customers'", connection);
        var custCols = new List<string>();
        using (var r = custColsCmd.ExecuteReader()) { while (r.Read()) custCols.Add(r.GetString(0)); }
        if (!custCols.Contains("CurrentBalance"))
        {
            try { using var c = new SqlCommand("ALTER TABLE Customers ADD CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0", connection); c.ExecuteNonQuery(); Console.WriteLine("Added CurrentBalance to Customers"); }
            catch (Exception ex) { Console.WriteLine("Could not add CurrentBalance to Customers: " + ex.Message); }
        }

        var suppColsCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers'", connection);
        var suppCols = new List<string>();
        using (var r = suppColsCmd.ExecuteReader()) { while (r.Read()) suppCols.Add(r.GetString(0)); }
        if (!suppCols.Contains("CurrentBalance"))
        {
            try { using var c = new SqlCommand("ALTER TABLE Suppliers ADD CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0", connection); c.ExecuteNonQuery(); Console.WriteLine("Added CurrentBalance to Suppliers"); }
            catch (Exception ex) { Console.WriteLine("Could not add CurrentBalance to Suppliers: " + ex.Message); }
        }

    // basic stock reconciliation: compute expected from stockhistory
        // basic stock reconciliation: prefer using StockHistory.NewStock if present
        var colCheckCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StockHistory'", connection);
        var cols = new List<string>();
        using (var reader = colCheckCmd.ExecuteReader())
        {
            while (reader.Read()) cols.Add(reader.GetString(0));
        }

        Console.WriteLine("\nStockHistory columns: " + string.Join(", ", cols));

        // If essential StockHistory columns missing, attempt to alter table to add them
        var requiredStockCols = new[] { "ReferenceId", "QuantityIn", "QuantityOut", "PreviousStock", "NewStock" };
        var missing = requiredStockCols.Where(c => !cols.Contains(c)).ToArray();
        if (missing.Length > 0)
        {
            Console.WriteLine("StockHistory is missing columns: " + string.Join(", ", missing));
            Console.WriteLine("Attempting to ALTER TABLE to add missing StockHistory columns...");

            var alterStatements = new List<string>();
            if (!cols.Contains("ReferenceId")) alterStatements.Add("ALTER TABLE StockHistory ADD ReferenceId NVARCHAR(100) NULL");
            if (!cols.Contains("QuantityIn")) alterStatements.Add("ALTER TABLE StockHistory ADD QuantityIn INT NOT NULL DEFAULT 0");
            if (!cols.Contains("QuantityOut")) alterStatements.Add("ALTER TABLE StockHistory ADD QuantityOut INT NOT NULL DEFAULT 0");
            if (!cols.Contains("PreviousStock")) alterStatements.Add("ALTER TABLE StockHistory ADD PreviousStock INT NOT NULL DEFAULT 0");
            if (!cols.Contains("NewStock")) alterStatements.Add("ALTER TABLE StockHistory ADD NewStock INT NOT NULL DEFAULT 0");

            foreach (var sql in alterStatements)
            {
                try
                {
                    using var cmd = new SqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Executed: " + sql);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to execute: " + sql + " — " + ex.Message);
                }
            }

            // re-query columns
            cols.Clear();
            using (var reader = colCheckCmd.ExecuteReader())
            {
                while (reader.Read()) cols.Add(reader.GetString(0));
            }
            Console.WriteLine("Updated StockHistory columns: " + string.Join(", ", cols));

            // proceed with reconciliation if NewStock exists now
            if (cols.Contains("NewStock"))
            {
                var reconSql = @"
                    SELECT p.ProductId, p.ProductName, p.CurrentStock, ISNULL(sh.NewStock, 0) AS LastRecordedStock
                    FROM Products p
                    LEFT JOIN (
                        SELECT ProductId, NewStock, StockHistoryId FROM StockHistory sh1
                        WHERE StockHistoryId = (SELECT MAX(StockHistoryId) FROM StockHistory sh2 WHERE sh2.ProductId = sh1.ProductId)
                    ) sh ON p.ProductId = sh.ProductId
                    WHERE p.CurrentStock <> ISNULL(sh.NewStock, 0)
                ";

                using (var cmd = new SqlCommand(reconSql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\nStock reconciliation differences:");
                        var any = false;
                        var discrepIds = new List<int>();
                        while (reader.Read())
                        {
                            any = true;
                            var pid = reader.GetInt32(0);
                            discrepIds.Add(pid);
                            Console.WriteLine($"ProductId={pid}, Name={reader.GetString(1)}, Current={reader.GetInt32(2)}, LastRecorded={reader.GetInt32(3)}");
                        }
                        if (!any) Console.WriteLine("No discrepancies found.");

                        // Detailed diagnostics for each discrepant product
                        foreach (var pid in discrepIds)
                        {
                            Console.WriteLine($"\n--- Diagnostics for ProductId={pid} ---");
                            using var pCmd = new SqlCommand("SELECT ProductName, CurrentStock FROM Products WHERE ProductId = @Pid", connection);
                            pCmd.Parameters.AddWithValue("@Pid", pid);
                            using var pR = pCmd.ExecuteReader();
                            int currentStock = 0;
                            if (pR.Read()) { currentStock = pR.GetInt32(1); Console.WriteLine($"ProductName={pR.GetString(0)}, CurrentStock={currentStock}"); }
                            pR.Close();

                            using var sumCmd2 = new SqlCommand("SELECT ISNULL(SUM(COALESCE(QuantityIn, Quantity) - COALESCE(QuantityOut,0)),0) AS Net FROM StockHistory WHERE ProductId = @Pid", connection);
                            sumCmd2.Parameters.AddWithValue("@Pid", pid);
                            var netObj2 = sumCmd2.ExecuteScalar();
                            Console.WriteLine($"Net change from StockHistory (sum of deltas) = {Convert.ToInt32(netObj2 ?? 0)}");

                            // if there are no stockhistory rows, insert opening entry to reflect current stock
                            using var countCmd = new SqlCommand("SELECT COUNT(1) FROM StockHistory WHERE ProductId = @Pid", connection);
                            countCmd.Parameters.AddWithValue("@Pid", pid);
                            var countObj = countCmd.ExecuteScalar();
                            var cnt = Convert.ToInt32(countObj ?? 0);
                                Console.WriteLine($"StockHistory row count for product {pid} = {cnt}");
                                if (cnt == 0)
                            {
                                Console.WriteLine("No StockHistory rows found — inserting opening StockHistory entry.");
                                using var ins = new SqlCommand("INSERT INTO StockHistory (ProductId, ChangeType, Quantity, ReferenceId, QuantityIn, QuantityOut, PreviousStock, NewStock, ChangeDate, ChangedBy, Reason) VALUES (@Pid, @Type, 0, @Ref, @QIn, 0, 0, @New, @Date, @By, @Reason)", connection);
                                ins.Parameters.AddWithValue("@Pid", pid);
                                ins.Parameters.AddWithValue("@Type", "OPENING");
                                ins.Parameters.AddWithValue("@Ref", "INITIAL_OPENING");
                            ins.Parameters.AddWithValue("@QIn", currentStock);
                            ins.Parameters.AddWithValue("@New", currentStock);
                                ins.Parameters.AddWithValue("@Date", DateTime.Now);
                                ins.Parameters.AddWithValue("@By", Environment.UserName);
                                ins.Parameters.AddWithValue("@Reason", "Opening stock insert by DbCheck");
                                try { ins.ExecuteNonQuery(); Console.WriteLine("Inserted opening StockHistory row."); }
                                catch (Exception ex) { Console.WriteLine("Failed inserting opening StockHistory: " + ex.Message); }
                            }

                            Console.WriteLine("Last 10 StockHistory rows (latest first):");
                            using var hCmd = new SqlCommand("SELECT TOP 10 StockHistoryId, ChangeType, QuantityIn, QuantityOut, PreviousStock, NewStock, ChangeDate, ReferenceId FROM StockHistory WHERE ProductId = @Pid ORDER BY StockHistoryId DESC", connection);
                            hCmd.Parameters.AddWithValue("@Pid", pid);
                            using var hR = hCmd.ExecuteReader();
                            while (hR.Read())
                            {
                                Console.WriteLine($"Id={hR.GetInt32(0)}, Type={hR.GetString(1)}, QIn={hR.GetInt32(2)}, QOut={hR.GetInt32(3)}, Prev={hR.GetInt32(4)}, New={hR.GetInt32(5)}, Date={hR.GetDateTime(6)}, Ref={hR[7]}");
                            }
                            hR.Close();
                        }
                    }
            }
            else
            {
                Console.WriteLine("Could not add NewStock column; skipping reconciliation.");
            }
        }

    // Populate historical PreviousStock/NewStock for StockHistory by replaying deltas per product
    Console.WriteLine("\nPopulating historical StockHistory PreviousStock/NewStock values...");
    var prodIds = new List<int>();
    using (var cmd = new SqlCommand("SELECT ProductId, CurrentStock FROM Products", connection))
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read()) prodIds.Add(reader.GetInt32(0));
    }

    foreach (var pid in prodIds)
    {
        // compute net delta from history
        var sumCmd = new SqlCommand("SELECT ISNULL(SUM(COALESCE(QuantityIn, Quantity) - COALESCE(QuantityOut, 0)),0) FROM StockHistory WHERE ProductId = @Pid", connection);
        sumCmd.Parameters.AddWithValue("@Pid", pid);
        var netObj = sumCmd.ExecuteScalar();
        var net = Convert.ToInt32(netObj ?? 0);

        var currentCmd = new SqlCommand("SELECT CurrentStock FROM Products WHERE ProductId = @Pid", connection);
        currentCmd.Parameters.AddWithValue("@Pid", pid);
        var current = Convert.ToInt32(currentCmd.ExecuteScalar() ?? 0);

        var opening = current - net;

        // fetch rows ordered
        var rowsCmd = new SqlCommand("SELECT StockHistoryId, COALESCE(QuantityIn, Quantity) AS QIn, COALESCE(QuantityOut,0) AS QOut FROM StockHistory WHERE ProductId = @Pid ORDER BY StockHistoryId", connection);
        rowsCmd.Parameters.AddWithValue("@Pid", pid);
        var cumulative = opening;
        var updates = new List<(int id, int prev, int neu)>();
        using (var rdr = rowsCmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                var id = rdr.GetInt32(0);
                var qin = rdr.GetInt32(1);
                var qout = rdr.GetInt32(2);
                var delta = qin - qout;
                var prev = cumulative;
                var neu = prev + delta;
                updates.Add((id, prev, neu));
                cumulative = neu;
            }
        }

        // apply updates in a transaction per product
        using var tran = connection.BeginTransaction();
        try
        {
            foreach (var u in updates)
            {
                using var up = new SqlCommand("UPDATE StockHistory SET PreviousStock = @Prev, NewStock = @New WHERE StockHistoryId = @Id", connection, tran);
                up.Parameters.AddWithValue("@Prev", u.prev);
                up.Parameters.AddWithValue("@New", u.neu);
                up.Parameters.AddWithValue("@Id", u.id);
                up.ExecuteNonQuery();
            }
            tran.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed updating history for product {pid}: {ex.Message}");
            try { tran.Rollback(); } catch { }
        }
    }

    Console.WriteLine("Historical StockHistory population complete.");

    // Run reconciliation diagnostics regardless of whether we added columns
    if (cols.Contains("NewStock"))
    {
        var reconSql = @"
                    SELECT p.ProductId, p.ProductName, p.CurrentStock, ISNULL(sh.NewStock, 0) AS LastRecordedStock
                    FROM Products p
                    LEFT JOIN (
                        SELECT ProductId, NewStock, StockHistoryId FROM StockHistory sh1
                        WHERE StockHistoryId = (SELECT MAX(StockHistoryId) FROM StockHistory sh2 WHERE sh2.ProductId = sh1.ProductId)
                    ) sh ON p.ProductId = sh.ProductId
                    WHERE p.CurrentStock <> ISNULL(sh.NewStock, 0)
                ";

        using (var cmd = new SqlCommand(reconSql, connection))
        using (var reader = cmd.ExecuteReader())
        {
            Console.WriteLine("\nStock reconciliation differences (diagnostics):");
            var any = false;
            var discrepIds = new List<int>();
            while (reader.Read())
            {
                any = true;
                var pid = reader.GetInt32(0);
                discrepIds.Add(pid);
                Console.WriteLine($"ProductId={pid}, Name={reader.GetString(1)}, Current={reader.GetInt32(2)}, LastRecorded={reader.GetInt32(3)}");
            }
            if (!any) Console.WriteLine("No discrepancies found (diagnostics).");

            foreach (var pid in discrepIds)
            {
                Console.WriteLine($"\n--- Diagnostics for ProductId={pid} ---");
                using var pCmd = new SqlCommand("SELECT ProductName, CurrentStock FROM Products WHERE ProductId = @Pid", connection);
                pCmd.Parameters.AddWithValue("@Pid", pid);
                using var pR = pCmd.ExecuteReader();
                int currentStock = 0;
                if (pR.Read()) { currentStock = pR.GetInt32(1); Console.WriteLine($"ProductName={pR.GetString(0)}, CurrentStock={currentStock}"); }
                pR.Close();

                using var sumCmd2 = new SqlCommand("SELECT ISNULL(SUM(COALESCE(QuantityIn, Quantity) - COALESCE(QuantityOut,0)),0) AS Net FROM StockHistory WHERE ProductId = @Pid", connection);
                sumCmd2.Parameters.AddWithValue("@Pid", pid);
                var netObj2 = sumCmd2.ExecuteScalar();
                Console.WriteLine($"Net change from StockHistory (sum of deltas) = {Convert.ToInt32(netObj2 ?? 0)}");
                    // check count and insert opening row if none
                    using var countCmd = new SqlCommand("SELECT COUNT(1) FROM StockHistory WHERE ProductId = @Pid", connection);
                    countCmd.Parameters.AddWithValue("@Pid", pid);
                    var countObj = countCmd.ExecuteScalar();
                    var cnt = Convert.ToInt32(countObj ?? 0);
                    Console.WriteLine($"StockHistory row count for product {pid} = {cnt}");
                    if (cnt == 0)
                    {
                        Console.WriteLine("No StockHistory rows found — inserting opening StockHistory entry.");
                        using var ins = new SqlCommand("INSERT INTO StockHistory (ProductId, ChangeType, Quantity, ReferenceId, QuantityIn, QuantityOut, PreviousStock, NewStock, ChangeDate, ChangedBy, Reason) VALUES (@Pid, @Type, 0, @Ref, @QIn, 0, 0, @New, @Date, @By, @Reason)", connection);
                        ins.Parameters.AddWithValue("@Pid", pid);
                        ins.Parameters.AddWithValue("@Type", "OPENING");
                        ins.Parameters.AddWithValue("@Ref", "INITIAL_OPENING");
                        ins.Parameters.AddWithValue("@QIn", currentStock);
                        ins.Parameters.AddWithValue("@New", currentStock);
                        ins.Parameters.AddWithValue("@Date", DateTime.Now);
                        ins.Parameters.AddWithValue("@By", Environment.UserName);
                        ins.Parameters.AddWithValue("@Reason", "Opening stock insert by DbCheck");
                        try { ins.ExecuteNonQuery(); Console.WriteLine("Inserted opening StockHistory row."); }
                        catch (Exception ex) { Console.WriteLine("Failed inserting opening StockHistory: " + ex.Message); }
                    }

                    Console.WriteLine("Last 10 StockHistory rows (latest first):");
                using var hCmd = new SqlCommand("SELECT TOP 10 StockHistoryId, ChangeType, QuantityIn, QuantityOut, PreviousStock, NewStock, ChangeDate, ReferenceId FROM StockHistory WHERE ProductId = @Pid ORDER BY StockHistoryId DESC", connection);
                hCmd.Parameters.AddWithValue("@Pid", pid);
                using var hR = hCmd.ExecuteReader();
                while (hR.Read())
                {
                    Console.WriteLine($"Id={hR.GetInt32(0)}, Type={hR.GetString(1)}, QIn={hR.GetInt32(2)}, QOut={hR.GetInt32(3)}, Prev={hR.GetInt32(4)}, New={hR.GetInt32(5)}, Date={hR.GetDateTime(6)}, Ref={hR[7]}");
                }
                hR.Close();
            }
        }
    }

        

    // check invoice counts
    var invoiceCounts = new[] { ("PurchaseInvoices","SELECT COUNT(1) FROM PurchaseInvoices"), ("SalesInvoices","SELECT COUNT(1) FROM SalesInvoices") };
    foreach (var (name, sql) in invoiceCounts)
    {
        try
        {
            using var cmd = new SqlCommand(sql, connection);
            var cnt = (int)cmd.ExecuteScalar();
            Console.WriteLine($"{name} count: {cnt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name} check failed: {ex.Message}");
        }
    }

    // audit trail recent
    using (var cmd = new SqlCommand("SELECT TOP 10 AuditId, Action, Entity, Reference, PerformedBy, PerformedAt FROM AuditTrail ORDER BY PerformedAt DESC", connection))
    using (var reader = cmd.ExecuteReader())
    {
        Console.WriteLine("\nRecent audit entries:");
        while (reader.Read())
        {
            Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetString(2)} | {reader[3]} | {reader[4]} | {reader.GetDateTime(5)}");
        }
    }

    Console.WriteLine("\nBasic DB checks completed.");
}
catch (Exception ex)
{
    Console.WriteLine("Error connecting or executing checks: " + ex.Message);
    Environment.ExitCode = 2;
}
