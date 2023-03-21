using System;
using System.Linq;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class TestGuidRandomness
{   
    [Theory]
    [InlineData("")]
    [InlineData("-13040")]
    [InlineData("51dd1872-14cc-48f7-bd22-767d82152cef")]
    public void TestWithFunRandomness(string randomness)
    {
        var guids = Enumerable.Range(0, 100_000_000)
            .AsParallel()
            .Select(_ => PercentageFlag.CalculateValue(Guid.NewGuid().ToString(), randomness))
            .GroupBy(x => x)
            .Select(grouping => (key: grouping.Key, count: grouping.Count()))
            .ToList();

        foreach (var res in guids)
        {
            // just sanity checking that newGuids, with or without some randomSeed mixed in, will spread mostly evenly when mod 100..
            // 100 mil guids split 100 ways -> expected 1 mil in each group (+/- 5_000)
            Assert.True(Math.Abs(1_000_000 - res.count) < 5_000); 
        }
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("-5123692")]
    [InlineData("f0dd1872-14cc-48f7-bdf2-111d82152cef")]
    public void TestWithFunRandomnessInt(string randomness)
    {
        var guids = Enumerable.Range(-50_000_000, 100_000_000)
            .AsParallel()
            .Select(x => PercentageFlag.CalculateValue(x.ToString(), randomness))
            .GroupBy(x => x)
            .Select(grouping => (key: grouping.Key, count: grouping.Count()))
            .ToList();

        foreach (var res in guids)
        {
            // just sanity checking that ints, with or without some randomSeed mixed in, will spread mostly evenly when mod 100..
            // 100 mil guids split 100 ways -> expected 1 mil in each group (+/- 5_000)
            Assert.True(Math.Abs(1_000_000 - res.count) < 5_000); 
        }
    }
}