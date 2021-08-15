namespace Veff.Internal.Responses
{
    public class FeatureContainerViewModel
    {
        public FeatureContainerViewModel(FeatureFlagViewModel[] flags)
        {
            Flags = flags;
        }

        public FeatureFlagViewModel[] Flags { get; set; }
    }
}