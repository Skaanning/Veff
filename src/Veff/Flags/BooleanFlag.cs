using System;
using System.Data;
using System.Data.SqlClient;
using Veff.Internal;

namespace Veff.Flags
{
    public class BooleanFlag : Flag
    {
        internal BooleanFlag(
            int id,
            string name,
            string description,
            bool isEnabled,
            IVeffSqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Id = id;
            Name = name;
            Description = description;
            _cachedValueExpiry = DateTimeOffset.UtcNow;
            _cachedValue = isEnabled;
        }

        public int Id { get; }
        internal string Name { get; }
        public string Description { get; }
        public bool IsEnabled => InternalIsEnabled();
        public bool IsDisabled => !IsEnabled;

        private bool InternalIsEnabled()
        {
            if (DateTimeOffset.UtcNow <= _cachedValueExpiry) return _cachedValue;

            var newValue = GetValueFromDb();

            _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffSqlConnectionFactory.CacheExpiry.TotalSeconds);
            _cachedValue = newValue;

            return _cachedValue;
        }

        private bool GetValueFromDb()
        {
            using var connection = VeffSqlConnectionFactory.UseConnection();

            using var cmd = new SqlCommand(@"
SELECT [Percent]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", connection);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            var percent = (int)cmd.ExecuteScalar();
            return percent == 100;
        }

        private DateTimeOffset _cachedValueExpiry;
        private bool _cachedValue;

        /// <summary>
        /// Useful for initializing nullable reference types so compiler doesnt complain
        /// It will be overwritten with the actual value from db before it will ever be used.
        /// <example> <code>
        /// public class MyFeatures : IFeatureContainer
        /// {
        ///     public BooleanFlag MyFlag { get; } = BooleanFlag.Empty;
        /// }
        /// </code> </example>
        /// </summary>
        public static BooleanFlag Empty { get; } = new(-1, "", "", false, null!);

    }
}