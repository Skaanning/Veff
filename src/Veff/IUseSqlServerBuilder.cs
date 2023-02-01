namespace Veff;

public interface IUseSqlServerBuilder
{
    IFeatureFlagContainerBuilder WithSqlServer(
        string connectionString);
}

public interface IUseSqliteBuilder
{
    IFeatureFlagContainerBuilder WithSqlite(
        string connectionString);
}