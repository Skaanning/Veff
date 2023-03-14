using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests;

public class TestRandomness
{
    [Fact]
    public void Test()
    {
        var guids = Enumerable.Range(0, 100_000_000).Select(_ => Guid.NewGuid()).ToArray();
        var groupBy = guids.Select(x => Math.Abs(x.GetHashCode() % 100)).GroupBy(x => x);
        var stringlist = new List<string>();
        foreach (var grouping in groupBy)
        {
            stringlist.Add($"{grouping.Key} = {grouping.Count()}");
        }
        Assert.True(true);
    }
}