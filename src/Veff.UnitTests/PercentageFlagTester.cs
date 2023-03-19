using System;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class PercentageFlagTester
{
    private readonly Guid _guid;
    private readonly int _expectedBreakingPoint;

    public PercentageFlagTester()
    {
        _guid = Guid.Parse("4b4f8bcd-03f4-42b6-a552-4c21dd340f9a");
        
        // Guids get converted to a value [0....99] in this case it is 59
        _expectedBreakingPoint = Math.Abs(_guid.GetHashCode()) % 100;
        Assert.Equal(59, _expectedBreakingPoint);
    }
    
    [Fact]
    public void ExpectedBehaviourWithoutRandomSeed()
    {
        // Some sanity checking
        // percentage value is between 0-100 where 0 is always false and 100 is always true.
        var isEnabled = PercentageFlag.InternalIsEnabled(_guid, 0, 0);
        Assert.False(isEnabled);

        isEnabled = PercentageFlag.InternalIsEnabled(_guid, 0, 100);
        Assert.True(isEnabled);

        isEnabled = PercentageFlag.InternalIsEnabled(_guid, 0, _expectedBreakingPoint);
        Assert.False(isEnabled);
        
        // For this GUID to be enabled the percentage would have to be 60% or higher :)
        isEnabled = PercentageFlag.InternalIsEnabled(_guid, 0, _expectedBreakingPoint + 1);
        Assert.True(isEnabled);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(823148159)]
    [InlineData(-60123)]
    [InlineData(-9469212)]
    [InlineData(249561052)]
    [InlineData(8520)]
    [InlineData(-8520)]
    public void MixupWithRandomnessSeed(int randomSeed)
    {
        // Since the same Users (via their Guids) would always be in the "target group" for a percentage flag
        // we can change it up by providing a randomSeed; 

        // sanity checking that 0% and 100% will always give us false and true 
        var isEnabled = PercentageFlag.InternalIsEnabled(_guid, randomSeed, 0);
        Assert.False(isEnabled);

        isEnabled = PercentageFlag.InternalIsEnabled(_guid, randomSeed, 100);
        Assert.True(isEnabled);

        var newBreakingPoint = PercentageFlag.CalculateValue(_guid.GetHashCode(), randomSeed);
        Assert.True(newBreakingPoint is >= 0 and < 100);
        
        isEnabled = PercentageFlag.InternalIsEnabled(_guid, randomSeed, newBreakingPoint);
        Assert.False(isEnabled);
        
        isEnabled = PercentageFlag.InternalIsEnabled(_guid, randomSeed, newBreakingPoint + 1);
        Assert.True(isEnabled);
    }
}