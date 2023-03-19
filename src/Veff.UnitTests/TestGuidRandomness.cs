using System;
using System.Linq;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class TestGuidRandomness
{   
    [Theory]
    [InlineData(0)]
    [InlineData(1025812)]
    [InlineData(-1295)]
    [InlineData(4673209)]
    [InlineData(-91923785)]
    [InlineData(91)]
    public void TestWithFunRandomness(int randomness)
    {
        var guids = Enumerable.Range(0, 100_000_000)
            .AsParallel()
            .Select(_ => PercentageFlag.CalculateValue(Guid.NewGuid().GetHashCode(), randomness))
            .GroupBy(x => x)
            .Select(grouping => (key: grouping.Key, count: grouping.Count()))
            .ToList();

        foreach (var res in guids)
        {
            // just sanity checking that newGuids will spread mostly evenly when mod 100..
            // 100 mil guids split 100 ways -> expected 1 mil in each group (+/- 5_000)
            Assert.True(Math.Abs(1_000_000 - res.count) < 5_000); 
        }
    }
}