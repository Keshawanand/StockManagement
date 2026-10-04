using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public class CategoryService
{
    public DataTable GetCategories()
    {
        return SqlHelper.ExecuteDataTable("SELECT CategoryId, CategoryName FROM Categories ORDER BY CategoryName");
    }

    public void AddCategory(string categoryName)
    {
        SqlHelper.ExecuteNonQuery(
            "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)",
            new SqlParameter("@CategoryName", categoryName));
    }

    public void UpdateCategory(int categoryId, string categoryName)
    {
        SqlHelper.ExecuteNonQuery(
            "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId",
            new SqlParameter("@CategoryId", categoryId),
            new SqlParameter("@CategoryName", categoryName));
    }

    public void DeleteCategory(int categoryId)
    {
        SqlHelper.ExecuteNonQuery(
            "DELETE FROM Categories WHERE CategoryId = @CategoryId",
            new SqlParameter("@CategoryId", categoryId));
    }
}
