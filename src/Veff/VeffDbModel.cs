using System;
using System.Linq;
using Veff.Flags;
using Veff.Internal;

namespace Veff
{
    internal class VeffDbModel
    {
        private readonly IVeffDbConnectionFactory _connectionFactory;

        internal VeffDbModel(
            int id,
            string name,
            string description,
            int percent,
            string type,
            string strings,
            IVeffDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            Id = id;
            Name = name;
            Description = description;
            Percent = percent;
            Type = type;
            Strings = strings.Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray();
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
            if (Type.Equals(typeof(BooleanFlag).FullName))
                return new BooleanFlag(Id, Name, Description, Percent == 100, _connectionFactory);
            if (Type.Equals(typeof(StringFlag).FullName))
                return new StringFlag(Id, Name, Description, Strings, _connectionFactory);

            throw new Exception($"Unknown type: {Type}");
        }
    }
}