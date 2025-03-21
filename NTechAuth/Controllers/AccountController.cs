using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using NTechAuth.Database;
using NTechAuth.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using QRCoder;
using NTechAuth.Components.Images;
using NTechAuth.Utilities;

namespace NTechAuth.Controllers
{
    public class AccountController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
    {
        [HttpPost("~/api/login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginRequestModel model)
        {
            if (model.Username == null || model.Password == null)
            {
                return BadRequest("Invalid Credentials");
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Username || u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return BadRequest("Invalid Credentials");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Id),
                new Claim("Username", user.Username),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var userRoles = context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Role.Name).ToList();
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return Ok(model.ReturnUrl);
            }

            return Ok("/");
        }

        [HttpGet("api/logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");
        }

        [HttpGet("api/account/GenerateQRCode")]
        [Authorize]
        public async Task<ActionResult> GenerateQRCode()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.IsMfaEnabled)
            {
                return Unauthorized("MFA has already been enabled on this account.");
            }

            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretKey);

            user.MfaSecretKey = base32Secret;
            await context.SaveChangesAsync();

            // Generer TOTP URI
            var totpUri = $"otpauth://totp/{user.Email}?secret={base32Secret}&issuer=MyApp";

            // Generer QR-koden ved hjælp af QRCoder
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q))
            {
                QRCode qrCode = new QRCode(qrCodeData);
                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    using (var ms = new MemoryStream())
                    {
                        qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Seek(0, SeekOrigin.Begin);

                        return File(ms.ToArray(), "image/png");
                    }
                }
            }
        }

        [HttpPost("api/account/avatar")]
        [Authorize]
        public async Task<ActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Upload en gyldig fil.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var rootPath = Path.Combine(environment.ContentRootPath);
            var filePath = Path.Combine(rootPath, "files", "avatars", $"{userId}.png");

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { filePath });
        }

        [HttpGet("api/account/avatar")]
        public async Task<ActionResult> GetAvatar([FromQuery] string? userId)
        {
            Console.WriteLine(userId ?? "null");
            var rootPath = Path.Combine(environment.ContentRootPath);

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound("User has no avatar");
                }
            }

            var avatarPath = Path.Combine(rootPath, "files", "avatars", $"{userId}.png");

            if (System.IO.File.Exists(avatarPath))
            {
                return PhysicalFile(avatarPath, "image/png");
            }

            return NotFound("User has no avatar");
        }
    }
}
