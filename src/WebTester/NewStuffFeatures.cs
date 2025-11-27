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
//     public required PercentageFlag SometimesIWork { get; init; }
//     public required StringEqualsFlag Baz111 { get; init; } 
//     public required StringContainsFlag Baz333 { get; init; } 
//     public required StringStartsWithFlag Baz555 { get; init; } 
//     public required StringEndsWithFlag EndsWith { get; init; }
}