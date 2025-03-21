using NTechAuth.Database;
using NTechAuth.Models.Database;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseExtensions
    {
        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!context.Roles.Any() && !context.Users.Any() && !context.UserRoles.Any())
                {
                    string adminRoleId = Guid.NewGuid().ToString();

                    context.Roles.AddRange(
                        new Role { Id = adminRoleId, Name = "Admin"}
                    );

                    var defaultSection = configuration.GetSection("Defaults");

                    string adminUserId = Guid.NewGuid().ToString();
                    string password = defaultSection["Password"]!;
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    context.Users.AddRange(
                        new User
                        {
                            Id = adminUserId,
                            Username = defaultSection["Username"]!,
                            FirstName = defaultSection["FirstName"]!,
                            LastName = defaultSection["LastName"]!,
                            Email = defaultSection["Email"]!,
                            Password = hashedPassword
                        }
                    );

                    context.UserRoles.AddRange(
                        new UserRole { UserId = adminUserId, RoleId = adminRoleId }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}
