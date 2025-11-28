using Veff;
using Veff.Flags;
using Veff.Flags.Attributes;

namespace WebTester;

public class NewStuffFeatures : IFeatureFlagContainer
{
     [FlagName("SendSomeEmails")]
     [InitialFlagValue(true)]
     public required BooleanFlag Hello { get; init; }
     
     [InitialFlagValue(true)]
     [FlagName("Blabla")]
     public required BooleanFlag CanUseEmails { get; init; }
     
     public required DateFlag SomeDateFeatureFlag { get; init; }
}