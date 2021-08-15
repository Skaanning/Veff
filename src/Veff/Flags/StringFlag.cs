using System;
using System.Collections.Generic;
using System.Linq;

namespace Veff.Flags
{
    public class StringFlag : Flag
    {
        internal readonly HashSet<string> Values;

        internal StringFlag(int id, string name, string description, params string[] values)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Id = id;
            Name = name;
            Description = description;
            Values = (values ?? Array.Empty<string>()).Select(x=>x.ToLower()).ToHashSet();
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool EnabledFor(string value) => Values.Contains(value.ToLower());
        public bool DisabledFor(string value) => !Values.Contains(value.ToLower());
        public bool EnabledForAny(params string[] values) => values.Any(x => Values.Contains(x.ToLower()));
        public bool EnabledForAll(params string[] values) => values.All(x => Values.Contains(x.ToLower()));
    }
}