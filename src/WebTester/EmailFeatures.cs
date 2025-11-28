using Veff;
using Veff.Flags;
using Veff.Flags.Attributes;

namespace WebTester;

public class EmailFeatures : IFeatureFlagContainer
{
    [InitialFlagValue(true)]
    [FlagName("SendSomeEmails", ContainerName = "test")]
    public required BooleanFlag SendSpamMails { get; set; }

    [InitialFlagValue("Bobby")]
    public required StringEqualsFlag SendActualEmails { get; set; } 
}