using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace NTechAuth.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> Test()
        {
            var scopeClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "scope")?.Value;
            var userScopes = scopeClaim.Split(' ');


            return Ok(userScopes);
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfileAsync()
        {
            // Hvis brugeren er autentificeret via en cookie
            if (User.Identity!.IsAuthenticated && User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                return Ok(new { message = "Access granted via login cookie." });
            }

            // Hvis brugeren er autentificeret via en access token
            if (User.Identity.IsAuthenticated)
            {
                var scopeClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "scope")?.Value;
                var userScopes = scopeClaim.Split(' ');

                // Kontrollér for scopen 'avatar'
                if (userScopes.Contains("avatar"))
                {
                    var claims = new List<string>();
                    foreach (var claim in User.Claims)
                    {
                        claims.Add($"{claim.Type}: {claim.Value}");
                    }
                    return Ok(new { message = "Access granted via access token with 'avatar' scope.", Claims = claims });
                }
                else
                {
                    return Forbid("Access token does not include the required 'avatar' scope.");
                }
            }

            // Hvis ingen af de ovenstående gælder
            return Unauthorized("User is not authenticated.");
        }
    }
}
