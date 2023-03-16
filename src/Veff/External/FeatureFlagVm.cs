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
}