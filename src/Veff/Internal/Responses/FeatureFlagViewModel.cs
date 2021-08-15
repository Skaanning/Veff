using System.Linq;
using Veff.Flags;

namespace Veff.Internal.Responses
{
    public class FeatureFlagViewModel
    {
        public FeatureFlagViewModel(Flag flag)
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
            if (flag is StringFlag f)
            {
                ContainerName = f.Name.Split('.')[0];
                Name = f.Name.Split('.')[1];
                Strings = string.Join("\n", f.Values.ToArray());
                Type = nameof(StringFlag);
                Description = f.Description;
                Id = f.Id;
            }
            if (flag is PercentFlag p)
            {
                ContainerName = p.Name.Split('.')[0];
                Name = p.Name.Split('.')[1];
                Percent = p.Percent;
                Type = nameof(PercentFlag);
                Description = p.Description;
                Id = p.Id;
            }
        }

        public int Id { get; }
        public string ContainerName { get; }
        public string Name { get; }
        public string Description { get; }
        public string Type { get; }
        public int Percent { get; }
        public bool Enabled { get; }
        public string Strings { get; }
    }
}