using Microsoft.EntityFrameworkCore;
using Netcorext.EntityFramework.UserIdentityPattern.Interceptors;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ServiceCollectionExtension
{
    private const long DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD = 1000;
    
    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityDbContext>(services, null, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        => AddIdentityDbContext<IdentityDbContext>(services, optionsAction, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext => AddIdentityDbContext<TContext>(services, null, lifetime, slowCommandLoggingThreshold);

    public static IServiceCollection AddIdentityDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped, long slowCommandLoggingThreshold = DEFAULT_SLOW_COMMAND_LOGGING_THRESHOLD)
        where TContext : DatabaseContext
    {
        services.AddHttpContextAccessor();

        services.AddDbContext<DatabaseContext, TContext>((provider, builder) =>
                                                         {
                                                             var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                                                             builder.AddInterceptors(new SlowCommandLoggingInterceptor(loggerFactory, slowCommandLoggingThreshold));
                                                             optionsAction?.Invoke(provider, builder);
                                                         }, lifetime);

        return services;
    }
}