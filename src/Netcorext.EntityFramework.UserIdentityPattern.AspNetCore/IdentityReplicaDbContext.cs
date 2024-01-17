using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Netcorext.EntityFramework.UserIdentityPattern.Entities;

namespace Netcorext.EntityFramework.UserIdentityPattern;

public class IdentityReplicaDbContext<T> : DatabaseContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityReplicaDbContext(DbContextOptions<IdentityReplicaDbContext<T>> options) : base(options)
    {
        _httpContextAccessor = this.GetService<IHttpContextAccessor>();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateBaseInfo();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess, Action<Entity>? handlerBase)
    {
        UpdateBaseInfo(handlerBase);

        return base.SaveChanges(acceptAllChangesOnSuccess, handlerBase);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        UpdateBaseInfo();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, Action<Entity>? handlerBase, CancellationToken cancellationToken = default)
    {
        UpdateBaseInfo(handlerBase);

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, handlerBase, cancellationToken);
    }

    private void UpdateBaseInfo(Action<Entity>? handlerBase = null)
    {
        var changedEntities = ChangeTracker.Entries<Entity>()
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
                                            entry.Entity.RequestId = GetRequestId();
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
                                            entry.Entity.RequestId = GetRequestId();
                                            entry.Entity.Version += 1;

                                            handlerBase?.Invoke(entry.Entity);

                                            break;
                                    }
                                });
    }

    private long GetUserId()
    {
        return long.TryParse(_httpContextAccessor.HttpContext?.User.Identity?.Name ?? "0", out var userId)
                   ? userId
                   : 0;
    }

    private string? GetRequestId()
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;

        var requestId = string.Empty;

        if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Request-Id", out var reqId))
        {
            requestId = reqId.ToString();
        }
        else if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Request-Id", out reqId))
        {
            requestId = reqId.ToString();
        }

        return string.IsNullOrWhiteSpace(requestId)
                   ? _httpContextAccessor.HttpContext.TraceIdentifier
                   : requestId;
    }
}
