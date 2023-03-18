using Veff;
using Veff.Flags;

namespace WebTester;

public class EmailFeatures : IFeatureFlagContainer
{
    public BooleanFlag SendSpamMails { get; } = BooleanFlag.Empty;
    public PercentageFlag IncludeFunnyCatPictures { get; } = PercentageFlag.Empty;
    public StringEqualsFlag SendActualEmails { get; } = StringEqualsFlag.Empty;
}