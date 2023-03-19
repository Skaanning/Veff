using System;
using System.Collections.Generic;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class StringEndsWithTester
{
    private readonly StringEndsWithFlag _flag;
    private readonly HashSet<string> _hashSet;

    public StringEndsWithTester()
    {
        _flag = StringEndsWithFlag.Empty;
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
    [InlineData("Ab")]
    [InlineData("dE")]
    [InlineData("DE")]
    [InlineData("mnA")]
    [InlineData("mN")]
    public void StartsWith_False(string value)
    {
        Assert.False(_flag.InternalIsEnabled(value, _hashSet));
    }
    
    [Theory]
    [InlineData("bc")]
    [InlineData("BC")]
    [InlineData("EF")]
    [InlineData("f")]
    [InlineData("no")]
    [InlineData("O")]
    public void EndsWith_True(string value)
    {
        Assert.True(_flag.InternalIsEnabled(value, _hashSet));
    }
}