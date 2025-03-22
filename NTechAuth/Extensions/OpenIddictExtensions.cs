using Microsoft.AspNetCore.Authentication.Cookies;
using NTechAuth.Database;

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
                    options
                        .AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();

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
                });

            //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            //{
            //    options.Authority = builder.Configuration["OpenId:Authority"]!;
            //    options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = false,
            //        ValidateLifetime = true,
            //        ValidIssuer = builder.Configuration["OpenId:Authority"]!,
            //        ClockSkew = TimeSpan.Zero
            //    };
            //    //options.Events = new JwtBearerEvents
            //    //{
            //    //    OnTokenValidated = context =>
            //    //    {
            //    //        Console.WriteLine("Token validated with Bearer.");
            //    //        return Task.CompletedTask;
            //    //    }
            //    //};
            //});

            return services;
        }
    }
}
