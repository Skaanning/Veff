namespace Veff.Flags.Mocked;

public class MockedStringEqualsFlag : StringEqualsFlag
{
    public MockedStringEqualsFlag(
        params string[] values) : base(0, "mocked", "mocked", values, null!)
    {
    }
}