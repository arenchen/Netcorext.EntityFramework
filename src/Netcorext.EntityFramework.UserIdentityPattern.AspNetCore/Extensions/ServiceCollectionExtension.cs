using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Netcorext.Contracts;
using Netcorext.EntityFramework.UserIdentityPattern.Interceptors;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ServiceCollectionExtension
{
    private const long DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD = 1000;

    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityReplicaDbContext<MasterContext>>(services, null, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityReplicaDbContext<MasterContext>>(services, optionsAction, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentitySlaveDbContext(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityReplicaDbContext<SlaveContext>>(services, null, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentitySlaveDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityReplicaDbContext<SlaveContext>>(services, optionsAction, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext =>
        AddIdentityDbContext<TContext>(services, null, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext
    {
        services.AddContextState();

        services.AddDbContext<TContext>((provider, builder) =>
                                        {
                                            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                                            builder.AddInterceptors(new SlowCommandLoggingInterceptor(loggerFactory, slowCommandLoggingThreshold));
                                            optionsAction?.Invoke(provider, builder);
                                        }, lifetime);

        services.Add(new ServiceDescriptor(typeof(DatabaseContext),
                                           p => p.GetService<TContext>()!,
                                           lifetime));

        services.TryAdd(new ServiceDescriptor(typeof(DatabaseContextAdapter), typeof(DatabaseContextAdapter), lifetime));

        return services;
    }

    public static IServiceCollection AddIdentityDbContextPool(this IServiceCollection services, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContextPool<IdentityReplicaDbContext<MasterContext>>(services, null, poolSize, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContextPool(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContextPool<IdentityReplicaDbContext<MasterContext>>(services, optionsAction, poolSize, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentitySlaveDbContextPool(this IServiceCollection services, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContextPool<IdentityReplicaDbContext<SlaveContext>>(services, null, poolSize, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentitySlaveDbContextPool(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContextPool<IdentityReplicaDbContext<SlaveContext>>(services, optionsAction, poolSize, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContextPool<TContext>(this IServiceCollection services, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext =>
        AddIdentityDbContextPool<TContext>(services, null, poolSize, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContextPool<TContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, int poolSize = 1024, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext
    {
        services.AddContextState();

        services.AddDbContextPool<TContext>((provider, builder) =>
                                            {
                                                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                                                builder.AddInterceptors(new SlowCommandLoggingInterceptor(loggerFactory, slowCommandLoggingThreshold));
                                                optionsAction?.Invoke(provider, builder);
                                            }, poolSize);

#pragma warning disable EF1001
        services.AddScoped<DatabaseContext>(provider => provider.GetRequiredService<IScopedDbContextLease<TContext>>().Context);

        services.TryAdd(new ServiceDescriptor(typeof(DatabaseContextAdapter), typeof(DatabaseContextAdapter), ServiceLifetime.Scoped));

        return services;
    }
}
