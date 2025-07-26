// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using WebQuark.QueryString;

namespace WebQuark.Tests
{
    public class QueryStringHandlerTests
    {
        [Fact]
        public void Get_ReturnsExpectedValue()
        {
            // Arrange
            var handler = new QueryStringHandler();
            handler.Set("key1", "value1");

            // Act
            var result = handler.Get("key1");

            // Assert
            Assert.Equal("value1", result);
        }

        [Fact]
        public void Get_WithDefault_ReturnsDefaultIfMissing()
        {
            var handler = new QueryStringHandler();
            var result = handler.Get("missingKey", "default");

            Assert.Equal("default", result);
        }

        [Fact]
        public void HasKey_ReturnsTrueIfExists()
        {
            var handler = new QueryStringHandler();
            handler.Set("exists", "yes");

            Assert.True(handler.HasKey("exists"));
        }

        [Fact]
        public void ToDictionary_ReturnsCorrectPairs()
        {
            var handler = new QueryStringHandler();
            handler.Set("a", "1");
            handler.Set("b", "2");

            var dict = handler.ToDictionary();

            Assert.Equal(2, dict.Count);
            Assert.Equal("1", dict["a"]);
        }
    }
}
