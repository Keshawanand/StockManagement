using System.Data;
using System.Data.SqlClient;

namespace StockManagementApp.DataAccess;

public static class SqlHelper
{
    public static string ConnectionString =>
        $"Server={(Environment.GetEnvironmentVariable("STOCK_DB_SERVER") ?? "localhost")};Database={(Environment.GetEnvironmentVariable("STOCK_DB_NAME") ?? "StockManagementDb")};Integrated Security=true;TrustServerCertificate=true;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public static int ExecuteNonQuery(string query, SqlConnection connection, SqlTransaction transaction, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddRange(parameters);
        return command.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        return command.ExecuteScalar();
    }

    public static object? ExecuteScalar(string query, SqlConnection connection, SqlTransaction transaction, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddRange(parameters);
        return command.ExecuteScalar();
    }

    public static DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static DataTable ExecuteDataTable(string query, SqlConnection connection, SqlTransaction transaction, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void ExecuteTransaction(Action<SqlConnection, SqlTransaction> action)
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            action(connection, transaction);
            transaction.Commit();
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
            }
            throw;
        }
    }
}
