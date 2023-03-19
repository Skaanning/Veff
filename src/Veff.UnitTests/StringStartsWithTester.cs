using System;
using System.Collections.Generic;
using Veff.Flags;
using Xunit;

namespace Veff.UnitTests;

public class StringStartsWithTester
{
    private readonly StringStartsWithFlag _flag;
    private readonly HashSet<string> _hashSet;

    public StringStartsWithTester()
    {
        _flag = StringStartsWithFlag.Empty;
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
    [InlineData("mno")]
    [InlineData("mN")]
    public void DoesntEqual_ButStartsWith(string value)
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
    public void DoesntStartWith(string value)
    {
        Assert.False(_flag.InternalIsEnabled(value, _hashSet));
    }
}