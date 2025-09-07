namespace NTech.Auth.Utilities
{
    public static class ConfigurationUpdater
    {
        public static void UpdateConfiguration(IConfiguration configuration, string sectionName = "Environment", bool userSwarmSecrets = false)
        {
            var envSection = configuration.GetSection(sectionName);
            TraverseSection(configuration, envSection, "", userSwarmSecrets);
        }

        private static void TraverseSection(IConfiguration configuration, IConfigurationSection section, string parentPath, bool userSwarmSecrets)
        {
            foreach (var child in section.GetChildren())
            {
                // Kombiner nøglen med den overordnede sti
                string fullKey = string.IsNullOrEmpty(parentPath) ? child.Key : $"{parentPath}:{child.Key}";

                if (child.GetChildren().Any())
                {
                    // Hvis der er flere undersektioner, gå rekursivt videre
                    TraverseSection(configuration, child, fullKey, userSwarmSecrets);
                }
                else
                {
                    string? env = Environment.GetEnvironmentVariable(child.Value);
                    if (env != null) configuration[fullKey] = env;
                }
            }
        }
    }
}
