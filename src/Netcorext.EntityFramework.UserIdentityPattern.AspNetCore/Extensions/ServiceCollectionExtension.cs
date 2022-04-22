using Microsoft.EntityFrameworkCore;
using Netcorext.EntityFramework.UserIdentityPattern.AspNetCore.Helpers;
using Netcorext.EntityFramework.UserIdentityPattern.AspNetCore.Internals;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddUserIdentityPatternDbContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        => AddUserIdentityPatternDbContext<TContext>(services, null, lifetime);
    public static IServiceCollection AddUserIdentityPatternDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionsAction, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        services.AddHttpContextAccessor();

        services.AddTransient(provider =>
                              {
                                  var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

                                  return new UpdateBaseInfoInterceptor(httpContextAccessor);
                              });

        services.Add(new ServiceDescriptor(typeof(TContext), InterceptorHelper.GetInterceptorObject<TContext>, lifetime));

        if (optionsAction == null)
        {
            services.AddDbContext<TContext>(lifetime);
        }
        else
        {
            services.AddDbContext<TContext>(optionsAction, lifetime);
        }

        return services;
    }
}