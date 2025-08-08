namespace NTechAuth.Models.Blazor
{
    public class ApplicationModel
    {
        public string Id { get; set; }
        public string ApplicationType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientType { get; set; }
        public string ConsentType { get; set; }
        public string DisplayName { get; set; }
        public List<string> Permissions { get; set; }
        public List<string> PostLogoutRedirectUris { get; set; }
        public List<string> RedirectUris { get; set; }
    }

}
