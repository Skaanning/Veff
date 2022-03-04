using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Veff.Internal;

namespace Veff.Flags
{
    public class StringFlag : Flag
    {
        internal StringFlag(
            int id,
            string name,
            string description,
            string[] values,
            IVeffSqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Id = id;
            Name = name;
            Description = description;
            _cachedValueExpiry = DateTimeOffset.UtcNow;
            _cachedValue = (values)
                .Select(x => x.ToLower())
                .ToHashSet();
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }

        public bool EnabledFor(
            string value) => InternalIsEnabled(value.ToLower());

        public bool DisabledFor(
            string value) => !EnabledFor(value.ToLower());

        public bool EnabledForAny(
            params string[] values) => values.Any(x => EnabledFor(x.ToLower()));

        public bool EnabledForAll(
            params string[] values) => values.All(x => EnabledFor(x.ToLower()));

        private bool InternalIsEnabled(
            string value)
        {
            if (DateTimeOffset.UtcNow <= _cachedValueExpiry) return _cachedValue.Contains(value);

            using var connection = VeffSqlConnectionFactory.UseConnection();
            var newValue = GetValueFromDb(); // connection.

            _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffSqlConnectionFactory.CacheExpiry.TotalSeconds);
            _cachedValue = newValue;

            return _cachedValue.Contains(value);
        }

        private HashSet<string> GetValueFromDb()
        {
            using var connection = VeffSqlConnectionFactory.UseConnection();

            using var cmd = new SqlCommand(@"
SELECT [Strings]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", connection);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            var percent = (string)cmd.ExecuteScalar();
            return percent.Split(";", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToHashSet();
        }

        private DateTimeOffset _cachedValueExpiry;
        private HashSet<string> _cachedValue;

        public HashSet<string> Values => _cachedValue;

        /// <summary>
        /// Useful for initializing nullable reference types so compiler doesnt complain.
        /// It will be overwritten with the actual value from db before it will ever be used.
        /// <example> <code>
        /// public class MyFeatures : IFeatureContainer
        /// {
        ///     public StringFlag MyFlag { get; } = StringFlag.Empty;
        /// }
        /// </code> </example>
        /// </summary>
        public static StringFlag Empty { get; } = new(-1, "", "", Array.Empty<string>(), null!);
    }
}