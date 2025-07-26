// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using WebQuark.Core.Interfaces;

#if NETFRAMEWORK
using System.Web;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
#endif

namespace WebQuark.HttpResponse
{
    /// <summary>
    /// Provides an abstraction to manipulate the HTTP response object in both .NET Framework and .NET Core environments.
    /// </summary>
    public class HttpResponseHandler : IHttpResponseHandler
    {
#if NETCOREAPP
        private readonly HttpContext _context;

        public HttpResponseHandler(IHttpContextAccessor accessor)
        {
            _context = accessor?.HttpContext ?? throw new ArgumentNullException(nameof(accessor));
        }
#elif NETFRAMEWORK
        private readonly HttpResponse _response;

        public HttpResponseHandler()
        {
            _response = HttpContext.Current?.Response ?? throw new InvalidOperationException("HttpResponse not available");
        }
#endif

        public void SetStatusCode(int statusCode)
        {
#if NETCOREAPP
            _context.Response.StatusCode = statusCode;
#elif NETFRAMEWORK
            _response.StatusCode = statusCode;
#endif
        }

        public void SetHeader(string key, string value)
        {
#if NETCOREAPP
            _context.Response.Headers[key] = value;
#elif NETFRAMEWORK
            _response.Headers[key] = value;
#endif
        }

        public void Write(string content, string contentType = "text/plain")
        {
#if NETCOREAPP
            _context.Response.ContentType = contentType ?? "text/plain";
            // ASP.NET Core: WriteAsync is async, but we block here for simplicity
            _context.Response.WriteAsync(content).GetAwaiter().GetResult();
#elif NETFRAMEWORK
            _response.ContentType = contentType ?? "text/plain";
            _response.Write(content);
#endif
        }

        public void Redirect(string url)
        {
#if NETCOREAPP
            _context.Response.Redirect(url);
#elif NETFRAMEWORK
            _response.Redirect(url);
#endif
        }

        public void SetCookie(string key, string value, DateTime? expires = null)
        {
#if NETCOREAPP
            var cookieOptions = new CookieOptions();
            if (expires.HasValue)
                cookieOptions.Expires = expires.Value;

            _context.Response.Cookies.Append(key, value, cookieOptions);
#elif NETFRAMEWORK
            var cookie = new HttpCookie(key, value);
            if (expires.HasValue)
                cookie.Expires = expires.Value;

            _response.Cookies.Add(cookie);
#endif
        }

        public void Clear()
        {
#if NETCOREAPP
            _context.Response.Clear();
#elif NETFRAMEWORK
            _response.Clear();
#endif
        }

        public void End()
        {
#if NETCOREAPP
            // Not supported in ASP.NET Core. You can throw, log, or do nothing.
            // throw new NotSupportedException("Response.End() is not supported in ASP.NET Core.");
#elif NETFRAMEWORK
            _response.End();
#endif
        }

        public void SetContentType(string contentType)
        {
#if NETCOREAPP
            _context.Response.ContentType = contentType;
#elif NETFRAMEWORK
            _response.ContentType = contentType;
#endif
        }
    }
}