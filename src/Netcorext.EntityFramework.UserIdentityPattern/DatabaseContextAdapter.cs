namespace Netcorext.EntityFramework.UserIdentityPattern;

public class DatabaseContextAdapter
{
    public DatabaseContextAdapter(IEnumerable<DatabaseContext> contexts)
    {
        var databaseContexts = contexts.ToArray();
        Master = databaseContexts.First(c => !c.IsSlave);
        Slave = databaseContexts.FirstOrDefault(c => c.IsSlave) ?? Master;
    }

    public static implicit operator DatabaseContext(DatabaseContextAdapter adapter) => adapter.Master;

    public DatabaseContext Master { get; set; }
    public DatabaseContext Slave { get; set; }
}