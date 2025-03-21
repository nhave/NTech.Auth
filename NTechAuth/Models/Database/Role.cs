namespace NTechAuth.Models.Database
{
    public class Role
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserRole> Roles { get; set; }
    }
}
