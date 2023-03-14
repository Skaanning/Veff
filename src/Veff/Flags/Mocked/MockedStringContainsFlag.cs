namespace Veff.Flags.Mocked;

public class MockedStringContainsFlag : StringContainsFlag
{
    public MockedStringContainsFlag(
        params string[] values) : base(0, "mocked", "mocked", values, null!)
    {
    }
}