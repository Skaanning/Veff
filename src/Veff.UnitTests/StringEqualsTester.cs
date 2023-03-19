using System;
using System.Collections.Generic;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class StringEqualsTester
{
    private readonly StringEqualsFlag _stringEqualsFlag;
    private readonly HashSet<string> _hashSet;

    public StringEqualsTester()
    {
        _stringEqualsFlag = StringEqualsFlag.Empty;
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
    [InlineData("ABC")]
    [InlineData("DEF")]
    [InlineData("def")]
    [InlineData("mno")]
    [InlineData("MNO")]
    public void DoesEqual(string value)
    {
        Assert.True(_stringEqualsFlag.InternalIsEnabled(value, _hashSet));
    } 
    
    [Theory]
    [InlineData("ab")]
    [InlineData("AC")]
    [InlineData("DF")]
    [InlineData("ef")]
    [InlineData("no")]
    [InlineData("NO")]
    public void DoesntEqual(string value)
    {
        Assert.False(_stringEqualsFlag.InternalIsEnabled(value, _hashSet));
    }
}