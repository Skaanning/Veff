namespace Veff;

public interface IUseSqlServerBuilder
{
    IFeatureFlagContainerBuilder WithSqlServer(
        string connectionString);
}