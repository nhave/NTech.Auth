using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NTechAuth.Database;
using NTechAuth.Models.Database;
using System.Security.Cryptography;
using System.Text;
using static NTechAuth.Database.ApplicationDbContext;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIddictExtensions
    {
        public static IServiceCollection AddOpenIddictWithAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the EF Core stores/models.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();
                })
                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options
                        .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
                        .AllowClientCredentialsFlow()
                        .AllowRefreshTokenFlow();

                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserInfoEndpointUris("/connect/userinfo");

                    options.SetIssuer(new Uri(configuration["OpenId:Authority"]!));

                    // Encryption and signing of tokens
                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["OpenId:EncryptionKey"]!)));

                    var rsa = RSA.Create();
                    rsa.ImportFromPem(File.ReadAllText(configuration["OpenId:SigningKeyLocation"]!));

                    options.AddSigningKey(new RsaSecurityKey(rsa));

                    // Register scopes (permissions)
                    options.RegisterScopes("api", "avatar");

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableUserInfoEndpointPassthrough();

                    // Disable encryption
                    options.DisableAccessTokenEncryption();
                });

            services.AddAuthorization();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/auth/login-oidc";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = configuration["OpenId:Authority"]!;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        TokenDecryptionKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["OpenId:EncryptionKey"]!)),
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidIssuer = configuration["OpenId:Authority"]!,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
