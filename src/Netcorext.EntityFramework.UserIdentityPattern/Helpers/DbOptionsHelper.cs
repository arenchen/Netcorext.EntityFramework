using System.Data.Common;

namespace Netcorext.EntityFramework.UserIdentityPattern.Helpers;

public static class DbOptionsHelper
{
    public static string? GetConnectionString(string? connectionString)
    {
        return GetConnectionInfo(connectionString).ConnectionString;
    }

    internal static (bool Slave, bool RequestId, string? ConnectionString) GetConnectionInfo(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, false, null);

        var build = new DbConnectionStringBuilder
                    {
                        ConnectionString = connectionString
                    };

        if (build.TryGetValue("slave", out var slave))
            build.Remove("slave");

        if (build.TryGetValue("requestId", out var requestId))
            build.Remove("requestId");

        var isSlave = slave?.ToString()?.ToUpper();
        var enableRequestId = requestId?.ToString()?.ToUpper();

        var boolSlave = !string.IsNullOrWhiteSpace(isSlave) && (isSlave == "1" || isSlave == "Y" || isSlave == "YES" || isSlave == bool.TrueString.ToUpper());
        var boolRequestId = !string.IsNullOrWhiteSpace(enableRequestId) && (enableRequestId == "1" || enableRequestId == "Y" || enableRequestId == "YES" || enableRequestId == bool.TrueString.ToUpper());

        return (boolSlave, boolRequestId, build.ConnectionString);
    }
}
