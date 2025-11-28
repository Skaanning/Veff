using System.Linq;
using Veff.Abstractions;
using Veff.Flags;

namespace Veff.Extensions;

public static class FeatureFlagContainerExtensions
{
    public static FeatureFlag[] FromFeatureFlagContainers(params IFeatureFlagContainer[] container)
    {
        var flagType = typeof(Flag);
        
        return container
            .SelectMany(x =>
            {
                return x.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsAssignableTo(flagType) && p.PropertyType is
                        { IsAbstract: false, IsInterface: false })
                    .Select(p => (flag: p.GetValue(x) as Flag, containerName: x.GetType().Name));
            })
            .SelectToArray((x) => 
                new FeatureFlag
                {
                    ContainerName = x.containerName,
                    Name = x.flag!.Name.Split(".").Last(),
                    Description = x.flag.Description,
                    Type = x.flag.GetType().ToString().Split(".").Last(),
                    Values = x.flag.Values,
                    Percent = x.flag.Percent,
                    Date = x.flag.Date
                });
    }
}