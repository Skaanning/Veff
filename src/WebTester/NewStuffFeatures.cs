using Veff;
using Veff.Flags;

namespace WebTester;

public class NewStuffFeatures : IFeatureFlagContainer
{
    public BooleanFlag Hello { get; } = BooleanFlag.Empty;
    public BooleanFlag CanUseEmails { get; } = BooleanFlag.Empty;
    public PercentageFlag SometimesIWork { get; } = PercentageFlag.Empty;
    public StringEqualsFlag Baz111 { get; } = StringEqualsFlag.Empty;
    public StringContainsFlag Baz333 { get; } = StringContainsFlag.Empty;
    public StringStartsWithFlag Baz555 { get; } = StringStartsWithFlag.Empty;
    public StringEndsWithFlag EndsWith { get; } = StringEndsWithFlag.Empty;
}