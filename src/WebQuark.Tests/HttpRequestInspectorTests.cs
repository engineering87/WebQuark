// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Http;
using WebQuark.HttpRequest;

namespace WebQuark.Tests
{
    public class HttpRequestInspectorTests
    {
        [Fact]
        public void GetHttpMethod_ReturnsGet()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";

            var inspector = new HttpRequestInspector(new HttpContextAccessor { HttpContext = context });

            Assert.Equal("GET", inspector.GetHttpMethod());
        }
    }
}
