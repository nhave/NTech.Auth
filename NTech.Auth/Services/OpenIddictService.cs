using Microsoft.EntityFrameworkCore;
using NTech.Auth.Database;
using NTech.Auth.Models.Blazor;
using NTech.Auth.Models.Database;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NTech.Auth.Services
{
    public class OpenIddictService(IOpenIddictApplicationManager manager, ApplicationDbContext context)
    {
        public async Task<List<ApplicationModel>> GetApplicationsAsync()
        {
            var applications = new List<ApplicationModel>();

            await foreach (var application in manager.ListAsync())
            {
                applications.Add(new ApplicationModel
                {
                    Id = await manager.GetIdAsync(application),
                    ClientId = await manager.GetClientIdAsync(application),
                    ClientType = await manager.GetClientTypeAsync(application),
                    DisplayName = await manager.GetDisplayNameAsync(application),
                    RedirectUris = (await manager.GetRedirectUrisAsync(application)).ToList(),
                    Permissions = (await manager.GetPermissionsAsync(application)).ToList(),
                    PostLogoutRedirectUris = (await manager.GetPostLogoutRedirectUrisAsync(application)).ToList(),
                    //Scopes = (await manager.GetScopesAsync(application)).ToList(),
                    //Type = await manager.GetTypeAsync(application)
                });
            }

            return applications;
        }

        public async Task<List<User>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<List<OpenIddictEntityFrameworkCoreApplication>> GetApplications()
        {
            return await context.OpenIddictApplications.ToListAsync();
        }

        public async Task<List<OpenIddictEntityFrameworkCoreScope>> GetScopes()
        {
            return await context.OpenIddictScopes.ToListAsync();
        }
    }
}
