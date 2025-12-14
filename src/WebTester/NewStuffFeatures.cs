using Veff;
using Veff.Flags;
using Veff.Flags.Attributes;
using Veff.Snapshot;

namespace WebTester;

public class NewStuffFeatures : IFeatureFlagContainer
{
     [FlagName("SendSomeEmails")]
     [InitialFlagValue(true)]
     public required BooleanFlag Hello { get; init; }
     
     public required BooleanFlag SendCatPictures { get; init; } 
     
     [InitialFlagValue("Bobby")]
     public required StringEqualsFlag SendActualEmails { get; init; } 

     [InitialFlagValue(null, "2025/11/30")]
     public required DateFlag SomeDateFeatureFlag { get; init; }
}

public class EmailFeatures : IFeatureFlagContainer
{
     [InitialFlagValue(true)]
     [FlagName("SendSomeEmails", ContainerName = "test")]
     public required BooleanFlag SendSpamMails { get; set; }
     public required DateFlag SomeDateFeatureFlag { get; init; }
     public required StringEndsWithFlag EndingFlag { get; init; }

}

public class EmailFeatureSnapshot : IFeatureFlagSnapshotContainer
{
     public required BooleanFlagSnapshot SendSpamMails { get; set; }
     public required DateFlagSnapshot SomeDateFeatureFlag { get; init; }
     public required StringEndsWithFlagSnapshot EndingFlag { get; init; }
}