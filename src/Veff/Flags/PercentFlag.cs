using System;

namespace Veff.Flags
{
    public class PercentFlag : Flag
    {
        private readonly Random _rnd;

        internal PercentFlag(int id, string name, string description, int percent)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent));

            Id = id;
            Name = name;
            Description = description;
            Percent = percent;
            _rnd = new Random();
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        internal int Percent { get; }
        public bool IsEnabled => _rnd.Next(101) <= Percent;
        public bool IsDisabled => _rnd.Next(101) >= Percent;
    }
}