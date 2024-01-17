namespace Netcorext.EntityFramework.UserIdentityPattern;

public class DatabaseContextAdapter
{
    private readonly IEnumerable<DatabaseContext> _contexts;

    public DatabaseContextAdapter(IEnumerable<DatabaseContext> contexts)
    {
        _contexts = contexts;

    }

    public static implicit operator DatabaseContext(DatabaseContextAdapter adapter) => adapter.Master;

    public DatabaseContext Master => _contexts.First(c => !c.IsSlave);
    public DatabaseContext Slave => _contexts.FirstOrDefault(c => c.IsSlave) ?? Master;
}