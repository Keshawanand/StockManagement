Summary of changes and how to run checks

- Implemented persistent invoice headers and items for purchases and sales (`PurchaseInvoices`, `SalesInvoices`, corresponding `*InvoiceItems`).
- Added multi-item invoice service methods: `PurchaseService.AddPurchaseInvoice(...)`, `SalesService.AddSalesInvoice(...)`.
- Integrated payment recording into invoice save (paid amount is recorded and `CurrentBalance` updated).
- Added `InvoiceNumber` persistence for legacy `Purchases`/`Sales` rows.
- Added `AuditTrail` and `AuditService` for centralized logging.
- Implemented invoice cancellation methods: `PurchaseService.CancelPurchaseInvoice(...)`, `SalesService.CancelSalesInvoice(...)` (reverses stock and creates reversal payment entries).
- Added invoice-level returns tables and methods: `PurchaseInvoiceReturns`, `SalesInvoiceReturns` and `AddReturnForInvoiceItem(...)` in return services.
- Added product validations (category existence, unique product code) in `ProductService`.
- Added `AuthenticationService` + `PasswordHelper` using PBKDF2; legacy plaintext passwords are auto-upgraded on first successful login.
- Added `StockReconciliationService` to detect and fix mismatches between `Products.CurrentStock` and `StockHistory`.
- Added `ReportService` with `GetCustomerBalances`, `GetSupplierBalances`, and `GetStockSummary`.

Quick checks (run from project folder):

```powershell
cd "d:\Stock Management\StockManagement\StockManagementApp"
dotnet build
```

How to run reconciliation and reports programmatically:
- Use `StockReconciliationService.GetDiscrepancies()` to view mismatches.
- Use `StockReconciliationService.FixDiscrepancy(productId)` to fix one product.
- Use `ReportService.GetCustomerBalances()` / `GetSupplierBalances()` / `GetStockSummary()` for quick reports.

Notes:
- Some UI updates (multi-line invoice entry forms and invoice selector UIs) are not yet added; backend support exists.
- Default users in DB were stored as plaintext in initial seed; first login will upgrade their stored password to a hashed value.
- Build succeeded locally with warnings about WinForms non-nullable fields (these are unchanged UI warnings).