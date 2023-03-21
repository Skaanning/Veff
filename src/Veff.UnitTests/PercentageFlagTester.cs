using System;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class PercentageFlagTester
{
    private readonly Guid _guid;
    private string GuidString => _guid.ToString();

    public PercentageFlagTester()
    {
        _guid = Guid.Parse("4b4f8bcd-03f4-42b6-a552-4c21dd340f9a");
    }

    [Theory]
    [InlineData("")]
    [InlineData("823148159")]
    [InlineData("-3148159")]
    [InlineData("Bobby Brown")]
    [InlineData("b6460072-6d63-492c-94c0-0e339dbbff41")]
    [InlineData("d13125f2-f5c8-4e42-ba57-952bfd12c552")]
    [InlineData("4b32e256-df40-4935-8f59-f8eb193a0e4f")]
    [InlineData("52488590-e33c-457a-9b06-bda16e6ffb41")]
    [InlineData("0ccd3a78-294a-472d-9468-08a5d586c466")]
    [InlineData("575c4898-2f1b-4e22-b0f8-ad9745f19f49")]
    [InlineData("c79ca57a-d4ca-4540-b9ec-8c1ae70c7a51")]
    [InlineData("31f65294-7123-4d32-9cf6-a874f608f00c")]
    [InlineData("49193317-e68a-42d7-8f6a-45de1c7fcc12")]
    [InlineData("5d098129-1d11-4001-a627-656f46d12f60")]
    public void MixupWithRandomnessSeed(string randomSeed)
    {
        // Since the same Users (via their Guids) would always be in the "target group" for a percentage flag
        // we can change it up by providing a randomSeed; 

        // sanity checking that 0% and 100% will always give us false and true 
        var isEnabled = PercentageFlag.InternalIsEnabled(GuidString, randomSeed, 0);
        Assert.False(isEnabled);

        isEnabled = PercentageFlag.InternalIsEnabled(GuidString, randomSeed, 100);
        Assert.True(isEnabled);

        var newBreakingPoint = PercentageFlag.CalculateValue(GuidString, randomSeed);
        Assert.True(newBreakingPoint is >= 0 and < 100);
        
        isEnabled = PercentageFlag.InternalIsEnabled(GuidString, randomSeed, newBreakingPoint);
        Assert.False(isEnabled);
        
        isEnabled = PercentageFlag.InternalIsEnabled(GuidString, randomSeed, newBreakingPoint + 1);
        Assert.True(isEnabled);
    }
}