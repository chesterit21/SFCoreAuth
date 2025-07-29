using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace SFCoreAuth.Infrastructure.Data;
public class SeedData : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedData(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SFCoreAuthDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        await CreateApplicationsAsync(scope.ServiceProvider, cancellationToken);
        await CreateRolesAsync(scope.ServiceProvider, cancellationToken);
        await CreateUsersAsync(scope.ServiceProvider, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateApplicationsAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        // MVC Client: mvc-um
        if (await manager.FindByClientIdAsync("mvc-um", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mvc-um",
                ClientSecret = "E1355416-52D5-4235-8A2B-7521364239A3", // Ganti dengan secret yang lebih kuat
                DisplayName = "MVC UM Application",
                RedirectUris = { new Uri("https://localhost:7001/signin-oidc") }, // Sesuaikan port client
                PostLogoutRedirectUris = { new Uri("https://localhost:7001/signout-callback-oidc") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            }, cancellationToken);
        }

        // MVC Client: mvc-pmcs
        if (await manager.FindByClientIdAsync("mvc-pmcs", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mvc-pmcs",
                ClientSecret = "A5B3A2C4-4A8E-4B7C-9F3D-3E1F7C5D8B6A", // Ganti dengan secret yang lebih kuat
                DisplayName = "MVC PMCS Application",
                RedirectUris = { new Uri("https://localhost:7002/signin-oidc") }, // Sesuaikan port client
                PostLogoutRedirectUris = { new Uri("https://localhost:7002/signout-callback-oidc") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            }, cancellationToken);
        }

        // SPA Client: spa-one (menggunakan PKCE, tanpa client secret)
        if (await manager.FindByClientIdAsync("spa-one", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "spa-one",
                DisplayName = "SPA One Application",
                RedirectUris = { new Uri("https://localhost:7003/callback") }, // Sesuaikan URL callback SPA
                PostLogoutRedirectUris = { new Uri("https://localhost:7003") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            }, cancellationToken);
        }

        // SPA Client: spa-pmcs
        if (await manager.FindByClientIdAsync("spa-pmcs", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "spa-pmcs",
                DisplayName = "SPA PMCS Application",
                RedirectUris = { new Uri("https://localhost:7004/callback") }, // Sesuaikan URL callback SPA
                PostLogoutRedirectUris = { new Uri("https://localhost:7004") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            }, cancellationToken);
        }
    }

    private async Task CreateRolesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var manager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "Operator" };
        foreach (var roleName in roleNames)
        {
            if (await manager.FindByNameAsync(roleName) is null)
            {
                await manager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private async Task CreateUsersAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var manager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        // Admin User
        var adminUser = new ApplicationUser
        {
            UserName = "adminapp@appone.id",
            Email = "adminapp@appone.id",
            FullName = "Application Admin",
            EmailConfirmed = true
        };
        if (await manager.FindByEmailAsync(adminUser.Email) is null)
        {
            var result = await manager.CreateAsync(adminUser, "#bebek123");
            if (result.Succeeded)
            {
                await manager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Operator User 1
        var opUser1 = new ApplicationUser
        {
            UserName = "op_one@appone.id",
            Email = "op_one@appone.id",
            FullName = "Operator Satu",
            EmailConfirmed = true
        };
        if (await manager.FindByEmailAsync(opUser1.Email) is null)
        {
            var result = await manager.CreateAsync(opUser1, "#bebek123");
            if (result.Succeeded)
            {
                await manager.AddToRoleAsync(opUser1, "Operator");
            }
        }

        // Operator User 2
        var opUser2 = new ApplicationUser
        {
            UserName = "op_two@appone.id",
            Email = "op_two@appone.id",
            FullName = "Operator Dua",
            EmailConfirmed = true
        };
        if (await manager.FindByEmailAsync(opUser2.Email) is null)
        {
            var result = await manager.CreateAsync(opUser2, "#bebek123");
            if (result.Succeeded)
            {
                await manager.AddToRoleAsync(opUser2, "Operator");
            }
        }
    }
}