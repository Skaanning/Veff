namespace Veff.Internal.Responses
{
    public class FeatureFlagViewModel
    {
        public FeatureFlagViewModel(int id, string containerName,
            string name,
            string description,
            string type,
            int percent,
            bool enabled,
            string strings)
        {
            Id = id;
            ContainerName = containerName;
            Name = name;
            Description = description;
            Type = type;
            Percent = percent;
            Enabled = enabled;
            Strings = strings;
        }

        public int Id { get; }
        public string ContainerName { get; } = "";
        public string Name { get; } = "";
        public string Description { get; } = "";
        public string Type { get; } = "";
        public int Percent { get; }
        public bool Enabled { get; }
        public string Strings { get; } = "";
    }
}