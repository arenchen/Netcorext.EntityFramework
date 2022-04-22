using Microsoft.EntityFrameworkCore;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder EnsureCreateUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        context.Database.EnsureCreated();

        return builder;
    }
    
    public static IApplicationBuilder EnsureDeleteUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        context.Database.EnsureDeleted();

        return builder;
    }
    
    public static IApplicationBuilder MigrateUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        context.Database.Migrate();

        return builder;
    }
}