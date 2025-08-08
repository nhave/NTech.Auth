namespace NTechAuth.Models.Blazor
{
    public class DisableMfaModel
    {
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string TotpCode { get; set; }
    }
}
