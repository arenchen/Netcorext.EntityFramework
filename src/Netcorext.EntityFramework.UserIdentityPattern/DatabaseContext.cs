using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Netcorext.EntityFramework.UserIdentityPattern.Entities;
using Netcorext.EntityFramework.UserIdentityPattern.Entities.Mapping;

namespace Netcorext.EntityFramework.UserIdentityPattern;

public abstract class DatabaseContext : DbContext
{
    protected DatabaseContext(DbContextOptions options) : base(options) { }

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
            Activator.CreateInstance(type, modelBuilder);
        }

        var entityTypes = modelBuilder.Model
                                      .GetEntityTypes()
                                      .ToArray();

        var fk = entityTypes.SelectMany(e => e.GetForeignKeys())
                            .OrderBy(k => k.ToString())
                            .ToArray();

        foreach (var relationship in fk)
        {
            var annotation = relationship.FindAnnotation(nameof(DeleteBehavior)) ?? new Annotation(nameof(DeleteBehavior), DeleteBehavior.Restrict);
            
            relationship.DeleteBehavior = (DeleteBehavior)annotation.Value!;
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
}