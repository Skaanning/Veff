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
            var fooBarFeatures = new FooBarFeatures
            {
                Foo = new MockedBooleanFlag(true),
                Baz = new MockedStringEqualsFlag("hello")
            };

            _sut = new MySuperService(fooBarFeatures);
        }

        [Fact]
        public void Test1()
        {
            var doStuff = _sut.DoStuff();
            Assert.Equal("Hello", doStuff);
        }
    }
}