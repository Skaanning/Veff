using Veff.Flags.Mocked;
using WebTest;
using WebTest.Controllers;
using Xunit;

namespace UnitTests
{
    public class MySuperServiceTest
    {
        private readonly MySuperService _sut;

        public MySuperServiceTest()
        {
            _sut = new MySuperService(new FooBarFeatures(
                new MockedBooleanFlag(true),
                new MockedPercentFlag(true),
                new MockedStringFlag("hello")));
        }
        
        [Fact]
        public void Test1()
        {
            var doStuff = _sut.DoStuff();
            Assert.Equal("Hello", doStuff);
        }
    }
}