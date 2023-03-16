namespace Veff.SqlServer;

public interface IUseSqlServerBuilder
{
    IFeatureFlagContainerBuilder WithSqlServer(
        string connectionString);
}