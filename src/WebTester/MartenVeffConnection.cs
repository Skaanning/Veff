using Marten;
using Veff.Dashboard;
using Veff.Persistence;

namespace WebTester;

public class MartenVeffConnection : IVeffConnection
{
    private readonly IDocumentStore _documentStore;

    public MartenVeffConnection(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public Task EnsureTablesExists()
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IVeffFlag>> GetAllValues()
    {
        await using var session = _documentStore.QuerySession();
        return await session.Query<MyVeffDbModel>().ToListAsync();
    }

    public async Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
    {
        await using var session = _documentStore.LightweightSession();
        var flag = await session.Query<MyVeffDbModel>().FirstAsync(x => x.Id == featureFlagUpdate.Id);

        flag.Strings = featureFlagUpdate.Strings.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        flag.Description = featureFlagUpdate.Description;
        flag.Percent = featureFlagUpdate.Percent;

        session.Store(flag);

        await session.SaveChangesAsync();
    }

    public async Task AddFlagsMissingInDb((string Name, string Type)[] flagsMissingInDb)
    {
        await using var session = _documentStore.LightweightSession();

        var myVeffDbModels = flagsMissingInDb.Select(x => new MyVeffDbModel 
        {
            Name = x.Name, 
            Description = "", 
            Percent = 0, 
            Type = x.Type, 
            Strings = Array.Empty<string>()
        });
        session.StoreObjects(myVeffDbModels);
        await session.SaveChangesAsync();
    }

    public HashSet<string> GetStringValueFromDb(int id, bool ignoreCase)
    {
        using var session = _documentStore.QuerySession();
        var strings = session.Query<MyVeffDbModel>().Where(x => x.Id == id).SelectMany(x => x.Strings).ToArray();
        return strings.ToHashSet(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
    }

    public int GetPercentValueFromDb(int id)
    {
        using var session = _documentStore.QuerySession();
        var percent = session.Query<MyVeffDbModel>().First(x => x.Id == id).Percent;
        return percent;
    }
    
    public void Dispose()
    {
        _documentStore.Dispose();
    }
}

public class MyVeffDbModel : IVeffFlag
{
    public string GetClassName()
        => Name.Split('.', 2)[0];

    public string GetPropertyName()
        => Name.Split('.', 2)[1];

    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Percent { get; set; }
    public string Type { get; set; } = "";
    public string[] Strings { get; set; } = Array.Empty<string>();
}