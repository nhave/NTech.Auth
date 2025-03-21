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
                // Kontrollér for scopen 'avatar'
                if (User.HasClaim("scope", "avatar"))
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
