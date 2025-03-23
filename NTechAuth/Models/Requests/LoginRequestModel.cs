using System.ComponentModel.DataAnnotations;

namespace NTechAuth.Models.Requests
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public string? totpCode { get; set; }

        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}
