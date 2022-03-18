using System.Reflection;
using Castle.DynamicProxy;
using Netcorext.EntityFramework.UserIdentityPattern.AspNetCore.Internals;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore.Helpers;

internal static class InterceptorHelper
{
    public static object GetInterceptorObject(Type serviceType, Type implementationType, IServiceProvider provider)
    {
        var generator = new ProxyGenerator();
        var constructors = ((TypeInfo)implementationType).DeclaredConstructors.Where(c => !c.IsStatic && c.IsPublic).ToArray();
        var ctor = constructors[0];
        var ctorParams = ctor.GetParameters().Select(pt => pt.ParameterType).ToArray();

        var injections = ctorParams.Select(p => ActivatorUtilities.GetServiceOrCreateInstance(provider, p))
                                   .ToArray();

        var interceptor = provider.GetRequiredService<UpdateBaseInfoInterceptor>();

        var interceptors = new IInterceptor[] { interceptor };

        var service = generator.CreateClassProxy(serviceType, new Type[] { implementationType }, ProxyGenerationOptions.Default, injections, interceptors);

        return service;
    }

    public static TService GetInterceptorObject<TService>(IServiceProvider provider)
    {
        return GetInterceptorObject<TService>(typeof(TService), provider);
    }

    public static TService GetInterceptorObject<TService>(Type implementationType, IServiceProvider provider)
    {
        return (TService)GetInterceptorObject(typeof(TService), implementationType, provider);
    }
}