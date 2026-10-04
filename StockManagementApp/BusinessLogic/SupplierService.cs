using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class SupplierService
{
    public DataTable GetSuppliers()
    {
        return SqlHelper.ExecuteDataTable("SELECT SupplierId, SupplierName, Mobile, Email, Address, GstNumber FROM Suppliers ORDER BY SupplierName");
    }

    public void AddSupplier(string supplierName, string mobile, string email, string address, string gstNumber)
    {
        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Suppliers (SupplierName, Mobile, Email, Address, GstNumber) VALUES (@SupplierName, @Mobile, @Email, @Address, @GstNumber)",
            new SqlParameter("@SupplierName", supplierName),
            new SqlParameter("@Mobile", mobile),
            new SqlParameter("@Email", email),
            new SqlParameter("@Address", address),
            new SqlParameter("@GstNumber", gstNumber));
    }

    public void UpdateSupplier(int supplierId, string supplierName, string mobile, string email, string address, string gstNumber)
    {
        SqlHelper.ExecuteNonQuery(
            "UPDATE Suppliers SET SupplierName = @SupplierName, Mobile = @Mobile, Email = @Email, Address = @Address, GstNumber = @GstNumber WHERE SupplierId = @SupplierId",
            new SqlParameter("@SupplierId", supplierId),
            new SqlParameter("@SupplierName", supplierName),
            new SqlParameter("@Mobile", mobile),
            new SqlParameter("@Email", email),
            new SqlParameter("@Address", address),
            new SqlParameter("@GstNumber", gstNumber));
    }

    public void DeleteSupplier(int supplierId)
    {
        SqlHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE SupplierId = @SupplierId", new SqlParameter("@SupplierId", supplierId));
    }

    public DataTable GetSupplierById(int supplierId)
    {
        return SqlHelper.ExecuteDataTable("SELECT SupplierId, SupplierName, Mobile, Email, Address, GstNumber FROM Suppliers WHERE SupplierId = @SupplierId", new SqlParameter("@SupplierId", supplierId));
    }
}
