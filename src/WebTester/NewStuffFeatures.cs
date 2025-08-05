using Veff;
using Veff.Flags;

namespace WebTester;

public class NewStuffFeatures : IFeatureFlagContainer
{
    public required BooleanFlag Hello { get; init; }
    public required BooleanFlag CanUseEmails { get; init; }
    public required PercentageFlag SometimesIWork { get; init; }
    public required StringEqualsFlag Baz111 { get; init; } 
    public required StringContainsFlag Baz333 { get; init; } 
    public required StringStartsWithFlag Baz555 { get; init; } 
    public required StringEndsWithFlag EndsWith { get; init; }
}