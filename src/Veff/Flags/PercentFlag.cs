using System;

namespace Veff.Flags
{
    public class PercentFlag : Flag
    {
        internal PercentFlag(int id, string name, string description, int percent)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent));

            Id = id;
            Name = name;
            Description = description;
            Percent = percent;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        internal int Percent { get; }
        public bool IsEnabled => new Random().Next(101) > Percent;
    }
}