using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Netcorext.EntityFramework.UserIdentityPattern.Entities;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore.Internals;

[Serializable]
internal class UpdateBaseInfoInterceptor : IInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateBaseInfoInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Intercept(IInvocation invocation)
    {
        var mi = invocation.MethodInvocationTarget;
        var parameters = mi.GetParameters();

        if (typeof(DbContext).IsAssignableFrom(invocation.TargetType)
         && (mi.Name.Equals(nameof(DatabaseContext.SaveChanges)) || mi.Name.Equals(nameof(DatabaseContext.SaveChangesAsync)))
         && parameters.FirstOrDefault(t => t.Name.Equals("acceptAllChangesOnSuccess", StringComparison.OrdinalIgnoreCase)) != null)
        {
            var dbContext = invocation.InvocationTarget as DbContext;
            var baseHandler = invocation.Arguments.FirstOrDefault(t => t is Action<Entity>) as Action<Entity>;
            UpdateBaseInfo(dbContext, baseHandler);
        }

        invocation.Proceed();

        var isAsync = typeof(Task).IsAssignableFrom(invocation.MethodInvocationTarget.ReturnType);

        if (isAsync)
        {
            invocation.ReturnValue = InterceptAsync((dynamic)invocation.ReturnValue);
        }
    }

    private static async Task InterceptAsync(Task task)
    {
        await task.ConfigureAwait(false);
        ;
    }

    private static async Task<T> InterceptAsync<T>(Task<T> task)
    {
        return await task.ConfigureAwait(false);
    }

    private void UpdateBaseInfo(DbContext context, Action<Entity>? handlerBase = null)
    {
        var changedEntities = context.ChangeTracker.Entries<Entity>()
                                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                                     .ToList();

        var userId = GetUserId();

        changedEntities.ForEach(entry =>
                                {
                                    //entry.Property(entity => entity.Version).IsModified = false;
                                    switch (entry.State)
                                    {
                                        case EntityState.Added:
                                            entry.Entity.CreationDate = DateTimeOffset.UtcNow;
                                            entry.Entity.CreatorId = userId;
                                            entry.Entity.ModificationDate = DateTimeOffset.UtcNow;
                                            entry.Entity.ModifierId = userId;
                                            entry.Entity.Version = 1;

                                            handlerBase?.Invoke(entry.Entity);

                                            break;
                                        case EntityState.Modified:
                                            var hasChange = entry.Properties.Any(p => p.CurrentValue != p.OriginalValue);

                                            if (!hasChange)
                                            {
                                                entry.State = EntityState.Unchanged;

                                                return;
                                            }

                                            entry.Entity.ModificationDate = DateTimeOffset.UtcNow;
                                            entry.Entity.ModifierId = userId;
                                            entry.Entity.Version += 1;

                                            handlerBase?.Invoke(entry.Entity);

                                            break;
                                    }
                                });
    }

    private long GetUserId()
    {
        return long.TryParse(_httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "0", out var userId)
                   ? userId
                   : 0;
    }
}