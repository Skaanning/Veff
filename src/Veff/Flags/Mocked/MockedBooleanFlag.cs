namespace Veff.Flags.Mocked;

public class MockedBooleanFlag : BooleanFlag
{
    public MockedBooleanFlag(
        bool isEnabled) : base(0, "mocked", "mocked", 100, null!)
    {
    }
}