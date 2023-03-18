using System;
using System.Linq;
using Veff.Extensions;
using Veff.Flags;
using Veff.Responses;

namespace Veff.External;

public class FeatureFlagVm
{
    public string ContainerName { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Type { get; init; } = "";

    public static FeatureFlagVm FromFeatureFlagViewModel(FeatureFlagViewModel viewModel)
    {
        return new FeatureFlagVm
        {
            ContainerName = viewModel.ContainerName,
            Name = viewModel.Name,
            Description = viewModel.Description,
            Type = viewModel.Type
        };
    }
    public static FeatureFlagVm[] FromFeatureFlagContainers(params IFeatureFlagContainer[] container)
    {
        return container
            .SelectMany(x =>
            {
                return x.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsAssignableTo(FlagType) && p.PropertyType is
                        { IsAbstract: false, IsInterface: false })
                    .Select(p => (flag: p.GetValue(x) as Flag, containerName: x.GetType().Name));
            })
            .SelectToArray((x) => 
                new FeatureFlagVm
                {
                    ContainerName = x.containerName,
                    Name = x.flag!.Name.Split(".").Last(),
                    Description = x.flag.Description,
                    Type = x.flag.GetType().ToString().Split(".").Last()
                });
    }
    
    private static Type FlagType = typeof(Flag);
}