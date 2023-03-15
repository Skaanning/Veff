using System.Linq;
using Veff.Flags;

namespace Veff.Internal.Responses
{
    public class FeatureFlagViewModel
    {
        public FeatureFlagViewModel(
            Flag flag)
        {
            if (flag is BooleanFlag b)
            {
                ContainerName = b.Name.Split('.')[0];
                Name = b.Name.Split('.')[1];
                Enabled = b.IsEnabled;
                Type = nameof(BooleanFlag);
                Description = b.Description;
                Id = b.Id;
            }

            if (flag is StringEqualsFlag f)
            {
                ContainerName = f.Name.Split('.')[0];
                Name = f.Name.Split('.')[1];
                Strings = string.Join("\n", f.Values.ToArray());
                Type = nameof(StringEqualsFlag);
                Description = f.Description;
                Id = f.Id;
            }
        }

        public int Id { get; }
        public string ContainerName { get; } = "";
        public string Name { get; } = "";
        public string Description { get; } = "";
        public string Type { get; } = "";
        // public int Percent { get; }
        public bool Enabled { get; }
        public string Strings { get; } = "";
    }
}