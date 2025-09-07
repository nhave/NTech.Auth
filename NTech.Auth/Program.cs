using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NTech.Auth.Components;
using NTech.Auth.Database;
using NTech.Auth.Services;

namespace NTech.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            builder.Services.AddOpenIddictWithAuth(builder.Configuration);

            builder.Services.AddControllers();

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<OpenIddictService>();

            builder.Services.AddHttpClient();

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
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            // Allows a status code page work with a layout.
            //app.UseStatusCodePagesWithReExecute("/error/{0}");

            //app.UseHttpsRedirection();

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
