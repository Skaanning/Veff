using System.Reflection;
using Marten;
using Veff.Dashboard;
using Veff.Flags.Attributes;
using Veff.Persistence;

namespace Veff.Marten;

public class VeffMartenConnection(IDocumentStore documentStore) : IVeffConnection
{
    public Task EnsureTablesExists()
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IVeffFlag>> GetAllValues()
    {
        using var session = documentStore.QuerySession();
        return await session.Query<MyVeffDbModel>().ToListAsync();
    }

    public async Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
    {
        using var session = documentStore.LightweightSession();
        var flag = await session.Query<MyVeffDbModel>().FirstAsync(x => x.Id == featureFlagUpdate.Id);

        flag.Strings = featureFlagUpdate.Strings.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        flag.Description = featureFlagUpdate.Description;
        flag.Percent = featureFlagUpdate.Percent;

        session.Store(flag);

        await session.SaveChangesAsync();
    }

    public async Task AddFlagsMissingInDb(
        (string AttrName, string Type, InitialFlagValue? initialValueFlag)[] flagsMissingInDb)
    {
        if (flagsMissingInDb.Length == 0)
            return;
        
        using var session = documentStore.LightweightSession();

        var myVeffDbModels = flagsMissingInDb.Select(x =>
        {
            var percentage = x.initialValueFlag?.Percentage ?? 0;
            var s = x.initialValueFlag?.Value;
            var strings = s == null 
                ? Array.Empty<string>() 
                : [s];

            return new MyVeffDbModel
            {
                Name = x.AttrName,
                Description = "",
                Percent = 0,
                Type = x.Type,
                Strings = strings
            };
        });
        
        session.StoreObjects(myVeffDbModels);
        await session.SaveChangesAsync();
    }

    public HashSet<string> GetStringValueFromDb(int id, bool ignoreCase)
    {
        using var session = documentStore.QuerySession();
        var strings = session.Query<MyVeffDbModel>().Where(x => x.Id == id).SelectMany(x => x.Strings).ToArray();
        return strings.ToHashSet(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
    }

    public int GetPercentValueFromDb(int id)
    {
        using var session = documentStore.QuerySession();
        var percent = session.Query<MyVeffDbModel>().First(x => x.Id == id).Percent;
        return percent;
    }

    public async Task RemoveFlagsNoLongerInCode(string[] allFlags)
    {
        if (allFlags.Length == 0) return;

        using var session = documentStore.DirtyTrackedSession();
        session.DeleteWhere<MyVeffDbModel>(x => !allFlags.Contains(x.Name));
        await session.SaveChangesAsync();
    }

    public string? GetOriginalStringValueFromDb(int id)
    {
        using var session = documentStore.QuerySession();
        var strings = session.Query<MyVeffDbModel>().Where(x => x.Id == id).SelectMany(x => x.Strings).ToArray();
        return string.Join(";", strings);
    }

    public void Dispose()
    {
        documentStore.Dispose();
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
    public string[] Strings { get; set; } = [];
}