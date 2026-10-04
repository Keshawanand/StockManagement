using System.Data;
using System.Data.SqlClient;
using StockManagementApp.DataAccess;

namespace StockManagementApp.BusinessLogic;

public static class AuditService
{
    public static void RecordAudit(SqlConnection connection, SqlTransaction transaction, string action, string entity, string reference, string details, string performedBy)
    {
        SqlHelper.ExecuteNonQuery(@"INSERT INTO AuditTrail (Action, Entity, Reference, Details, PerformedBy, PerformedAt)
                                    VALUES (@Action, @Entity, @Reference, @Details, @PerformedBy, GETDATE())",
            connection, transaction,
            new SqlParameter("@Action", action ?? string.Empty),
            new SqlParameter("@Entity", entity ?? string.Empty),
            new SqlParameter("@Reference", reference ?? string.Empty),
            new SqlParameter("@Details", details ?? string.Empty),
            new SqlParameter("@PerformedBy", performedBy ?? string.Empty));
    }
}
