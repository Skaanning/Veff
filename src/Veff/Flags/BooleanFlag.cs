using System;

namespace Veff.Flags
{
    public class BooleanFlag : Flag
    {
        internal BooleanFlag(int id, string name, string description, bool isEnabled)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Id = id;
            Name = name;
            Description = description;
            IsEnabled = isEnabled;
        }

        public int Id { get; }
        internal string Name { get; }
        public string Description { get; }
        public bool IsEnabled { get; }
    }
}