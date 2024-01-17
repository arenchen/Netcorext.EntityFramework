using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Netcorext.EntityFramework.UserIdentityPattern.Entities;
using Netcorext.EntityFramework.UserIdentityPattern.Entities.Mapping;

namespace Netcorext.EntityFramework.UserIdentityPattern;

public abstract class DatabaseContext : DbContext
{
    protected DatabaseContext(DbContextOptions options) : base(TrimDbContextOptions(options))
    {
        if (options.Extensions
                   .FirstOrDefault(t => t.GetType()
                                         .IsAssignableTo(typeof(RelationalOptionsExtension))) is not RelationalOptionsExtension ext)
            return;

        var build = new DbConnectionStringBuilder
                    {
                        ConnectionString = ext.ConnectionString
                    };

        if (build.TryGetValue("slave", out var slave))
            build.Remove("slave");

        if (build.TryGetValue("requestId", out var requestId))
            build.Remove("requestId");

        var isSlave = slave?.ToString()?.ToUpper();
        var enableRequestId = requestId?.ToString()?.ToUpper();

        IsSlave = !string.IsNullOrWhiteSpace(isSlave) && (isSlave == "1" || isSlave == "Y" || isSlave == "YES" || isSlave == bool.TrueString.ToUpper());
        EnableRequestId = !string.IsNullOrWhiteSpace(enableRequestId) && (enableRequestId == "1" || enableRequestId == "Y" || enableRequestId == "YES" || enableRequestId == bool.TrueString.ToUpper());
    }

    public bool IsSlave { get; }
    public bool EnableRequestId { get; set; }

    public virtual int SaveChanges(Action<Entity>? handlerBase)
    {
        return SaveChanges(true, handlerBase);
    }

    public virtual int SaveChanges(bool acceptAllChangesOnSuccess, Action<Entity>? handlerBase)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public virtual async Task<int> SaveChangesAsync(Action<Entity>? handlerBase, CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(true, handlerBase, cancellationToken);
    }

    public virtual async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, Action<Entity>? handlerBase, CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var types = GetEntityMapping();

        foreach (var type in types)
        {
            var map = Activator.CreateInstance(type, modelBuilder);

            if (map == null)
                continue;

            var property = type.GetProperty("Builder");

            if (property == null)
                continue;

            var builder = property.GetValue(map) as EntityTypeBuilder;

            if (builder == null)
                continue;

            if (!EnableRequestId)
                builder.Ignore(nameof(Entity.RequestId));
        }
    }

    private IEnumerable<Type> GetEntityMapping()
    {
        var baseType = typeof(EntityMap<>);

        var types = AppDomain.CurrentDomain.GetAssemblies()
                             .SelectMany(assembly => assembly.GetTypes())
                             .Where(type => !type.IsGenericType && type.IsClass && type.BaseType != null && type.BaseType.Name == baseType.Name);

        return types;
    }

    private static DbContextOptions TrimDbContextOptions(DbContextOptions options)
    {
        if (options.Extensions
                   .FirstOrDefault(t => t.GetType()
                                         .IsAssignableTo(typeof(RelationalOptionsExtension))) is not RelationalOptionsExtension ext)
            return options;

        var build = new DbConnectionStringBuilder
                    {
                        ConnectionString = ext.ConnectionString
                    };

        if (build.TryGetValue("slave", out var slave))
            build.Remove("slave");

        if (build.TryGetValue("requestId", out var requestId))
            build.Remove("requestId");

        ext = ext.WithConnectionString(build.ConnectionString);

        options = options.WithExtension(ext);

        return options;
    }
}
