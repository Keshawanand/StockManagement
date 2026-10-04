using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class CustomerService
{
    public DataTable GetCustomers()
    {
        return SqlHelper.ExecuteDataTable("SELECT CustomerId, CustomerName, Mobile, Email, Address, GstNumber FROM Customers ORDER BY CustomerName");
    }

    public void AddCustomer(string customerName, string mobile, string email, string address, string gstNumber)
    {
        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Customers (CustomerName, Mobile, Email, Address, GstNumber) VALUES (@CustomerName, @Mobile, @Email, @Address, @GstNumber)",
            new SqlParameter("@CustomerName", customerName),
            new SqlParameter("@Mobile", mobile),
            new SqlParameter("@Email", email),
            new SqlParameter("@Address", address),
            new SqlParameter("@GstNumber", gstNumber));
    }

    public void UpdateCustomer(int customerId, string customerName, string mobile, string email, string address, string gstNumber)
    {
        SqlHelper.ExecuteNonQuery(
            "UPDATE Customers SET CustomerName = @CustomerName, Mobile = @Mobile, Email = @Email, Address = @Address, GstNumber = @GstNumber WHERE CustomerId = @CustomerId",
            new SqlParameter("@CustomerId", customerId),
            new SqlParameter("@CustomerName", customerName),
            new SqlParameter("@Mobile", mobile),
            new SqlParameter("@Email", email),
            new SqlParameter("@Address", address),
            new SqlParameter("@GstNumber", gstNumber));
    }

    public void DeleteCustomer(int customerId)
    {
        SqlHelper.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerId = @CustomerId", new SqlParameter("@CustomerId", customerId));
    }

    public DataTable GetCustomerById(int customerId)
    {
        return SqlHelper.ExecuteDataTable("SELECT CustomerId, CustomerName, Mobile, Email, Address, GstNumber FROM Customers WHERE CustomerId = @CustomerId", new SqlParameter("@CustomerId", customerId));
    }
}
