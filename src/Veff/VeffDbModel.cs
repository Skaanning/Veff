using System;
using System.Linq;
using Veff.Flags;

namespace Veff
{
    internal class VeffDbModel
    {
        internal VeffDbModel(int id, string name, string description, int percent, string type, string strings)
        {
            Id = id;
            Name = name;
            Description = description;
            Percent = percent;
            Type = type;
            Strings = (strings ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        public string GetClassName()
            => Name.Split('.', 2)[0];
        
        public string GetPropertyName()
            => Name.Split('.', 2)[1];

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Percent { get; }
        public string Type { get; }
        public string[] Strings { get; }

        public Flag AsImpl()
        {
            if (Type.Equals(typeof(PercentFlag).FullName))
                return new PercentFlag(Id, Name, Description, Percent);
            if (Type.Equals(typeof(BooleanFlag).FullName))
                return new BooleanFlag(Id, Name, Description, Percent == 100);
            if (Type.Equals(typeof(StringFlag).FullName))
                return new StringFlag(Id, Name, Description, Strings);

            throw new Exception($"Unknown type: {Type}");
        }
    }
}