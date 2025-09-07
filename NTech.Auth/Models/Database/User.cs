namespace NTech.Auth.Models.Database
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public ICollection<UserRole> Roles { get; set; }

        // MFA-Section
        public bool IsMfaEnabled { get; set; }
        public string? MfaSecretKey { get; set; }
        public DateTime? MfaSetupDate { get; set; }
        public string? MfaBackupCodes { get; set; }
    }
}
