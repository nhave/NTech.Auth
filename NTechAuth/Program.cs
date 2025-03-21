using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NTechAuth.Components;
using NTechAuth.Database;
using NTechAuth.Services;
using NTechAuth.Utilities;

namespace NTechAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationUpdater.UpdateConfiguration(builder.Configuration);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            builder.Services.AddOpenIddict()
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
                    options.AllowClientCredentialsFlow();
                    options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

                    options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token");

                    options.SetIssuer(new Uri(builder.Configuration["OpenId:Authority"]!));

                    // Encryption and signing of tokens
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                    // Register scopes (permissions)
                    options.RegisterScopes("api", "avatar");
                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough();

                    // Disable encryption
                    options.DisableAccessTokenEncryption();
                });

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/auth/login-oidc";
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = builder.Configuration["OpenId:Authority"]!;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["OpenId:Authority"]!,
                        ClockSkew = TimeSpan.Zero
                    };
                    //options.Events = new JwtBearerEvents
                    //{
                    //    OnTokenValidated = context =>
                    //    {
                    //        Console.WriteLine("Token validated with Bearer.");
                    //        return Task.CompletedTask;
                    //    }
                    //};
                });

            builder.Services.AddControllers();

            builder.Services.AddHostedService<TestData>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Aktivate XForwardedProto Headers use with NGINX Reverse Proxy.
            if (app.Environment.IsProduction() || app.Environment.IsStaging())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto
                });
            }

            // Allows a status code page work with a layout.
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapControllers();

            app.InitializeDatabase();

            app.Run();
        }
    }
}
