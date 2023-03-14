namespace Veff.Flags.Mocked;

public class MockedStringEndsWithFlag : StringEndsWithFlag
{
    public MockedStringEndsWithFlag(
        params string[] values) : base(0, "mocked", "mocked", values, null!)
    {
    }
}