using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public static class PasswordHelper
{
    public static string HashPassword(string password, int iterations = 10000)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        // handle legacy plaintext stored password (no separators)
        if (!storedHash.Contains(':'))
            return password == storedHash;

        var parts = storedHash.Split(':');
        if (parts.Length != 3) return false;
        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(hash.Length);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}

public class AuthenticationService
{
    public bool CreateUser(string username, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required.", nameof(password));

        var hashed = PasswordHelper.HashPassword(password);
        SqlHelper.ExecuteNonQuery("INSERT INTO Users (UserName, PasswordHash, Role, IsActive, CreatedAt) VALUES (@UserName, @PasswordHash, @Role, 1, @CreatedAt)",
            new SqlParameter("@UserName", username),
            new SqlParameter("@PasswordHash", hashed),
            new SqlParameter("@Role", role ?? "Staff"),
            new SqlParameter("@CreatedAt", DateTime.Now));
        return true;
    }

    public bool ValidateUser(string username, string password, out string role)
    {
        role = string.Empty;
        var dt = SqlHelper.ExecuteDataTable("SELECT UserId, PasswordHash, Role FROM Users WHERE UserName = @UserName AND IsActive = 1", new SqlParameter("@UserName", username));
        if (dt.Rows.Count == 0) return false;

        var row = dt.Rows[0];
        var stored = row["PasswordHash"].ToString() ?? string.Empty;
        var isValid = PasswordHelper.VerifyPassword(password, stored);
        if (!isValid)
        {
            // if stored was plaintext, allow equality check already handled in VerifyPassword
            return false;
        }

        // if stored was plaintext (no ':'), upgrade to hashed
        if (!stored.Contains(':'))
        {
            var newHash = PasswordHelper.HashPassword(password);
            SqlHelper.ExecuteNonQuery("UPDATE Users SET PasswordHash = @Hash WHERE UserName = @UserName", new SqlParameter("@Hash", newHash), new SqlParameter("@UserName", username));
        }

        role = row["Role"].ToString() ?? string.Empty;
        return true;
    }
}
