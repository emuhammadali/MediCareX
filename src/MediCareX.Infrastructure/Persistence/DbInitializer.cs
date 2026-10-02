using MediCareX.Domain.Entities;
using MediCareX.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediCareX.Infrastructure.Persistence;

/// <summary>Runs at host start (not at design time): applies migrations, seeds roles + optional admin.</summary>
public sealed class DbInitializer(IServiceProvider services, IConfiguration config, ILogger<DbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (config.GetValue("Database:AutoMigrate", false))
                await db.Database.MigrateAsync(ct);

            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            foreach (var name in Enum.GetNames<UserRole>())
                if (!await roles.RoleExistsAsync(name)) await roles.CreateAsync(new ApplicationRole(name));

            var email = config["Seed:AdminEmail"];
            var password = config["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.FindByEmailAsync(email) is not null) return;

            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true,
                FirstName = "System", LastName = "Admin", IsActive = true
            };
            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded) await users.AddToRoleAsync(admin, nameof(UserRole.Admin));
            else logger.LogError("Admin seeding failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Code)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialisation failed. Did you run 'dotnet ef database update'?");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
