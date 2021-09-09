namespace Veff.Flags.Mocked
{
    public class MockedStringFlag : StringFlag
    {
        public MockedStringFlag(
            params string[] values) : base(0, "mocked", "mocked", values, null)
        {
        }
    }
}