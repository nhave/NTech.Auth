using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using NTechAuth.Database;
using NTechAuth.Models.Database;
using NTechAuth.Models.Requests;
using OtpNet;
using System.Reflection.Emit;
using System.Security.Claims;

namespace NTechAuth.Services
{
    public class UserService(ApplicationDbContext context, AuthenticationStateProvider authentication)
    {
        public async Task<User?> GetCurrenrUserAsync()
        {
            var authState = await authentication.GetAuthenticationStateAsync();

            if (authState.User.Identity != null && authState.User.Identity.IsAuthenticated)
            {
                var userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                var user = await context.Users.FindAsync(userId);

                return user;
            }

            return null;
        }

        public async Task<User?> ValidateLoginAsync(LoginRequestModel LoginModel)
        {
            if (LoginModel.Username == null || LoginModel.Password == null) return null;

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == LoginModel.Username || u.Username == LoginModel.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(LoginModel.Password, user.Password)) return null;

            return user;
        }

        public async Task<string?> GenerateMFASecretAsync()
        {
            var user = await GetCurrenrUserAsync();
            if (user == null || user.IsMfaEnabled) return null;

            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretKey);

            user.MfaSecretKey = base32Secret;
            await context.SaveChangesAsync();
            return base32Secret;
        }

        public async Task<bool> EnableMFAAsync(string totpCode)
        {
            var user = await GetCurrenrUserAsync();

            if (user == null || user.IsMfaEnabled || !VerifyTotp(user.MfaSecretKey!, totpCode))
            {
                return false;
            }

            user.IsMfaEnabled = true;
            user.MfaSetupDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DisableMFAAsync(string totpCode)
        {
            var user = await GetCurrenrUserAsync();

            if (user == null || !user.IsMfaEnabled)
            {
                return false;
            }

            user.IsMfaEnabled = false;
            user.MfaSetupDate = null;
            await context.SaveChangesAsync();
            return true;
        }

        public bool VerifyTotp(string secretKey, string totpCode)
        {
            if (secretKey == null || totpCode == null) return false;
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            return totp.VerifyTotp(totpCode, out long timeStepMatched, new VerificationWindow(1, 1));
        }
    }
}
