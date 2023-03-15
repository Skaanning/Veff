namespace Veff.Flags.Mocked;

public class MockedStringStartsWithFlag : StringStartsWithFlag
{
    public MockedStringStartsWithFlag(
        params string[] values) : base(0, "mocked", "mocked", values, null!)
    {
    }
}