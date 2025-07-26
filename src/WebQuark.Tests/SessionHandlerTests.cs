// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Http;
using WebQuark.Session;
using WebQuark.Tests.Model;

namespace WebQuark.Tests
{
    public class SessionHandlerTests
    {
        // Dummy implementation di IHttpContextAccessor per test
        private class TestHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext HttpContext { get; set; }
        }

        [Fact]
        public void Set_And_Get_String_Value()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Session = new DummySession();

            var contextAccessor = new TestHttpContextAccessor
            {
                HttpContext = context
            };

            var handler = new SessionHandler(contextAccessor);

            // Act
            handler.Set("username", "john");
            var result = handler.Get<string>("username");

            // Assert
            Assert.Equal("john", result);
        }
    }
}