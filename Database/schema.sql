IF DB_ID('StockManagementDb') IS NULL
BEGIN
    CREATE DATABASE StockManagementDb;
END
GO
USE StockManagementDb;
GO
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);
END
GO
IF OBJECT_ID('dbo.Suppliers', 'U') IS NULL
BEGIN
CREATE TABLE Suppliers (
    SupplierId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(150) NOT NULL,
    Mobile NVARCHAR(20),
    Email NVARCHAR(150),
    Address NVARCHAR(250),
    GstNumber NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    OpeningBalance DECIMAL(12,2) NOT NULL DEFAULT 0,
    CurrentBalance DECIMAL(12,2) NOT NULL DEFAULT 0
);
END
GO
IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(150) NOT NULL,
    Mobile NVARCHAR(20),
    Email NVARCHAR(150),
    Address NVARCHAR(250),
    GstNumber NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    OpeningBalance DECIMAL(12,2) NOT NULL DEFAULT 0,
    CurrentBalance DECIMAL(12,2) NOT NULL DEFAULT 0
);
END
GO
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL UNIQUE,
    Barcode NVARCHAR(50),
    CategoryName NVARCHAR(100),
    Brand NVARCHAR(100),
    Unit NVARCHAR(50),
    PurchasePrice DECIMAL(12,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(12,2) NOT NULL DEFAULT 0,
    GstPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    ReorderLevel INT NOT NULL DEFAULT 0,
    CurrentStock INT NOT NULL DEFAULT 0,
    Description NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO
IF OBJECT_ID('dbo.Purchases', 'U') IS NULL
BEGIN
CREATE TABLE Purchases (
    PurchaseId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    InvoiceNumber NVARCHAR(100) NULL,
    Quantity INT NOT NULL,
    PurchasePrice DECIMAL(12,2) NOT NULL,
    GstPercent DECIMAL(5,2) NOT NULL,
    Discount DECIMAL(12,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(12,2) NOT NULL,
    PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.Sales', 'U') IS NULL
BEGIN
CREATE TABLE Sales (
    SaleId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    InvoiceNumber NVARCHAR(100) NULL,
    Quantity INT NOT NULL,
    SellingPrice DECIMAL(12,2) NOT NULL,
    GstPercent DECIMAL(5,2) NOT NULL,
    Discount DECIMAL(12,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(12,2) NOT NULL,
    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.StockHistory', 'U') IS NULL
BEGIN
CREATE TABLE StockHistory (
    StockHistoryId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    ChangeType NVARCHAR(50) NOT NULL,
    ReferenceId NVARCHAR(100),
    Quantity INT NOT NULL,
    QuantityIn INT NOT NULL DEFAULT 0,
    QuantityOut INT NOT NULL DEFAULT 0,
    PreviousStock INT NOT NULL,
    NewStock INT NOT NULL,
    Reason NVARCHAR(250),
    ChangeDate DATETIME NOT NULL DEFAULT GETDATE(),
    ChangedBy NVARCHAR(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.StockAdjustment', 'U') IS NULL
BEGIN
CREATE TABLE StockAdjustment (
    AdjustmentId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    AdjustmentType NVARCHAR(20) NOT NULL,
    Quantity INT NOT NULL,
    Reason NVARCHAR(250),
    AdjustmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    ChangedBy NVARCHAR(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.SalesReturn', 'U') IS NULL
BEGIN
CREATE TABLE SalesReturn (
    SalesReturnId INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    Quantity INT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.PurchaseReturn', 'U') IS NULL
BEGIN
CREATE TABLE PurchaseReturn (
    PurchaseReturnId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseId INT NOT NULL,
    Quantity INT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO

IF OBJECT_ID('dbo.CustomerPayments', 'U') IS NULL
BEGIN
CREATE TABLE CustomerPayments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Amount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    Reference NVARCHAR(200),
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO

IF OBJECT_ID('dbo.SupplierPayments', 'U') IS NULL
BEGIN
CREATE TABLE SupplierPayments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Amount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    Reference NVARCHAR(200),
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO

-- Invoice header/detail tables for persistent multi-item invoices
IF OBJECT_ID('dbo.PurchaseInvoices', 'U') IS NULL
BEGIN
CREATE TABLE PurchaseInvoices (
    PurchaseInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DueAmount AS (TotalAmount - PaidAmount) PERSISTED,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    IsCancelled BIT NOT NULL DEFAULT 0
);
END
GO

IF OBJECT_ID('dbo.PurchaseInvoiceItems', 'U') IS NULL
BEGIN
CREATE TABLE PurchaseInvoiceItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseInvoiceId INT NOT NULL FOREIGN KEY REFERENCES PurchaseInvoices(PurchaseInvoiceId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    PurchasePrice DECIMAL(18,2) NOT NULL,
    GstPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(12,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL
);
END
GO

IF OBJECT_ID('dbo.SalesInvoices', 'U') IS NULL
BEGIN
CREATE TABLE SalesInvoices (
    SalesInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DueAmount AS (TotalAmount - PaidAmount) PERSISTED,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    IsCancelled BIT NOT NULL DEFAULT 0
);
END
GO

IF OBJECT_ID('dbo.AuditTrail', 'U') IS NULL
BEGIN
CREATE TABLE AuditTrail (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    Action NVARCHAR(100) NOT NULL,
    Entity NVARCHAR(100) NOT NULL,
    Reference NVARCHAR(200) NULL,
    Details NVARCHAR(MAX) NULL,
    PerformedBy NVARCHAR(200) NULL,
    PerformedAt DATETIME NOT NULL DEFAULT GETDATE()
);
END
GO

IF OBJECT_ID('dbo.SalesInvoiceItems', 'U') IS NULL
BEGIN
CREATE TABLE SalesInvoiceItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    SalesInvoiceId INT NOT NULL FOREIGN KEY REFERENCES SalesInvoices(SalesInvoiceId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    SellingPrice DECIMAL(18,2) NOT NULL,
    GstPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(12,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL
);
END
GO
-- Seed default users with temporary plaintext passwords.
-- The app auto-upgrades plaintext to PBKDF2-SHA256 on first successful login.
-- IMPORTANT: Change these passwords immediately after first login.
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'admin')
BEGIN
INSERT INTO Users (UserName, PasswordHash, Role, IsActive) VALUES
('admin', 'admin123', 'Admin', 1);
END
GO
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'staff')
BEGIN
INSERT INTO Users (UserName, PasswordHash, Role, IsActive) VALUES
('staff', 'staff123', 'Staff', 1);
END
GO
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
INSERT INTO Categories (CategoryName) VALUES
('Electronics'),
('Furniture'),
('Groceries');
END
GO
IF NOT EXISTS (SELECT 1 FROM Suppliers)
BEGIN
INSERT INTO Suppliers (SupplierName, Mobile, Email, Address, GstNumber) VALUES
('ABC Traders', '9876543210', 'abc@example.com', 'Delhi', 'GST123'),
('North Supplies', '9123456780', 'north@example.com', 'Mumbai', 'GST456');
END
GO
IF NOT EXISTS (SELECT 1 FROM Customers)
BEGIN
INSERT INTO Customers (CustomerName, Mobile, Email, Address, GstNumber) VALUES
('John Doe', '9090909090', 'john@example.com', 'Noida', 'CUST001'),
('Jane Smith', '8080808080', 'jane@example.com', 'Gurgaon', 'CUST002');
END
GO
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
INSERT INTO Products (ProductName, ProductCode, Barcode, CategoryName, Brand, Unit, PurchasePrice, SellingPrice, GstPercent, ReorderLevel, CurrentStock, Description) VALUES
('Laptop', 'LP001', 'BAR001', 'Electronics', 'Dell', 'Piece', 45000, 52000, 18, 5, 20, 'Business laptop'),
('Chair', 'CH001', 'BAR002', 'Furniture', 'IKEA', 'Piece', 2500, 3200, 12, 10, 15, 'Ergonomic chair');
END
GO

-- These tables reference PurchaseInvoiceItems/SalesInvoiceItems and must come after them
IF OBJECT_ID('dbo.PurchaseInvoiceReturns', 'U') IS NULL
BEGIN
CREATE TABLE PurchaseInvoiceReturns (
    PurchaseInvoiceReturnId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseInvoiceItemId INT NOT NULL FOREIGN KEY REFERENCES PurchaseInvoiceItems(ItemId),
    Quantity INT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO

IF OBJECT_ID('dbo.SalesInvoiceReturns', 'U') IS NULL
BEGIN
CREATE TABLE SalesInvoiceReturns (
    SalesInvoiceReturnId INT IDENTITY(1,1) PRIMARY KEY,
    SalesInvoiceItemId INT NOT NULL FOREIGN KEY REFERENCES SalesInvoiceItems(ItemId),
    Quantity INT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL
);
END
GO