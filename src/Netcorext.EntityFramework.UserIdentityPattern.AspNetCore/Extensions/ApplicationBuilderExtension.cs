using Microsoft.EntityFrameworkCore;

namespace Netcorext.EntityFramework.UserIdentityPattern.AspNetCore;

public static class ApplicationBuilderExtension
{
    public static string GenerateDdl<TEntity>(this IApplicationBuilder builder) => GenerateDdl(builder);
    public static string GenerateDdl(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContextAdapter>();

        return context.Master.Database.GenerateCreateScript();
    }
    
    public static bool EnsureCreateDatabase<TEntity>(this IApplicationBuilder builder) => EnsureCreateDatabase(builder);
    public static bool EnsureCreateDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContextAdapter>();

        return context.Master.Database.EnsureCreated();
    }
    
    public static bool EnsureDeleteDatabase<TEntity>(this IApplicationBuilder builder) => EnsureDeleteDatabase(builder);
    public static bool EnsureDeleteDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContextAdapter>();

        return context.Master.Database.EnsureDeleted();
    }

    public static void MigrateDatabase<TEntity>(this IApplicationBuilder builder) => MigrateDatabase(builder);
    public static void MigrateDatabase(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContextAdapter>();

        context.Master.Database.Migrate();
    }
}