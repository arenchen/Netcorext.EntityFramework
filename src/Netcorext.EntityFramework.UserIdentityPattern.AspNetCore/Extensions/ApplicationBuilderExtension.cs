using Microsoft.EntityFrameworkCore;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ApplicationBuilderExtension
{
    public static bool EnsureCreateUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        return context.Database.EnsureCreated();
    }
    
    public static bool EnsureDeleteUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        return context.Database.EnsureDeleted();
    }
    
    public static void MigrateUserIdentityPatternDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        
        context.Database.Migrate();
    }
}