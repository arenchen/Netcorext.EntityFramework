using Microsoft.EntityFrameworkCore;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        => AddIdentityDbContext<IdentityDbContext>(services, null, lifetime);

    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        => AddIdentityDbContext<IdentityDbContext>(services, optionsAction, lifetime);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TContext : DatabaseContext => AddIdentityDbContext<TContext>(services, null, lifetime);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TContext : DatabaseContext
    {
        services.AddHttpContextAccessor();

        if (optionsAction == null)
        {
            services.AddDbContext<DatabaseContext, TContext>(lifetime);
        }
        else
        {
            services.AddDbContext<DatabaseContext, TContext>(optionsAction, lifetime);
        }

        return services;
    }
}