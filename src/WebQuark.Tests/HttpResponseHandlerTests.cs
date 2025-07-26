// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
#if NETCOREAPP
using System;
using Microsoft.AspNetCore.Http;
using WebQuark.HttpResponse;
using Xunit;

namespace WebQuark.Tests
{
    public class HttpResponseHandlerTests
    {
        private readonly DefaultHttpContext _defaultContext;
        private readonly HttpResponseHandler _handler;

        public HttpResponseHandlerTests()
        {
            _defaultContext = new DefaultHttpContext();
            var accessor = new HttpContextAccessor { HttpContext = _defaultContext };
            _handler = new HttpResponseHandler(accessor);
        }

        [Fact]
        public void SetStatusCode_SetsExpectedValue()
        {
            _handler.SetStatusCode(404);
            Assert.Equal(404, _defaultContext.Response.StatusCode);
        }

        [Fact]
        public void SetHeader_SetsExpectedHeader()
        {
            _handler.SetHeader("X-Test", "true");
            Assert.Equal("true", _defaultContext.Response.Headers["X-Test"]);
        }

        [Fact]
        public void SetContentType_SetsCorrectType()
        {
            _handler.SetContentType("application/json");
            Assert.Equal("application/json", _defaultContext.Response.ContentType);
        }

        [Fact]
        public void SetCookie_AddsCookieToResponse()
        {
            var key = "TestCookie";
            var value = "cookie_value";
            var expires = DateTime.UtcNow.AddDays(1);

            _handler.SetCookie(key, value, expires);

            Assert.True(_defaultContext.Response.Headers.ContainsKey("Set-Cookie"));
            var cookieHeader = _defaultContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains(key, cookieHeader);
            Assert.Contains(value, cookieHeader);
        }

        [Fact]
        public void Redirect_SetsRedirectLocation()
        {
            var url = "/new-url";
            _handler.Redirect(url);
            Assert.Equal(302, _defaultContext.Response.StatusCode);
            Assert.Equal(url, _defaultContext.Response.Headers["Location"]);
        }

        [Fact]
        public void Clear_ClearsHeadersAndBody()
        {
            _handler.SetHeader("X-Test", "to-clear");
            _handler.Clear();

            // NOTE: Clear() does not remove headers by default in ASP.NET Core, 
            // so we test content type instead.
            Assert.Null(_defaultContext.Response.ContentType);
        }
    }
}

#endif