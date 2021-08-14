namespace Veff.Flags.Mocked
{
    public class MockedPercentFlag : PercentFlag
    {
        public MockedPercentFlag(bool enabled) : base(0, "mocked", "mocked", enabled ? 100 : 0)
        {
            
        }
        
        public MockedPercentFlag(int percent) : base(0, "mocked", "mocked", percent)
        {
            
        }
    }
}