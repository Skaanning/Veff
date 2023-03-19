using System;
using System.Collections.Generic;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class StringContainsTester
{
    private readonly StringContainsFlag _flag;
    private readonly HashSet<string> _hashSet;

    public StringContainsTester()
    {
        _flag = StringContainsFlag.Empty;
        _hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "abc",
            "def",
            "ghi",
            "jkl",
            "mno"
        };
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("aBc")]
    [InlineData("DeF")]
    [InlineData("def")]
    [InlineData("mno")]
    [InlineData("MNO")]
    public void DoesEqual(string value)
    {
        Assert.True(_flag.InternalIsEnabled(value, _hashSet));
    } 
    
    [Theory]
    [InlineData("ab")]
    [InlineData("BC")]
    [InlineData("DE")]
    [InlineData("ef")]
    [InlineData("no")]
    [InlineData("NO")]
    public void DoesntEqual_ButContains(string value)
    {
        Assert.True(_flag.InternalIsEnabled(value, _hashSet));
    }
    
    [Theory]
    [InlineData("ac")]
    [InlineData("AC")]
    [InlineData("DF")]
    [InlineData("ec")]
    [InlineData("mo")]
    [InlineData("MO")]
    public void DoesntContains(string value)
    {
        Assert.False(_flag.InternalIsEnabled(value, _hashSet));
    }
}