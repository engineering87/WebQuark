// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Threading.Tasks;
using WebQuark.Core.Interfaces;

#if NETFRAMEWORK
using System.Web;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseHandler"/> class using the provided HTTP context accessor.
        /// </summary>
        /// <param name="accessor">The HTTP context accessor.</param>
        public HttpResponseHandler(IHttpContextAccessor accessor)
        {
            _context = accessor?.HttpContext ?? throw new ArgumentNullException(nameof(accessor));
        }
#elif NETFRAMEWORK
        private readonly HttpResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseHandler"/> class using the current HTTP context.
        /// </summary>
        public HttpResponseHandler()
        {
            _response = HttpContext.Current?.Response ?? throw new InvalidOperationException("HttpResponse not available");
        }
#endif

        /// <summary>
        /// Sets the HTTP status code of the response.
        /// </summary>
        /// <param name="statusCode">The status code to set.</param>
        public void SetStatusCode(int statusCode)
        {
#if NETCOREAPP
            _context.Response.StatusCode = statusCode;
#elif NETFRAMEWORK
            _response.StatusCode = statusCode;
#endif
        }

        /// <summary>
        /// Sets or overrides a response header.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header value.</param>
        public void SetHeader(string key, string value)
        {
#if NETCOREAPP
            _context.Response.Headers[key] = value;
#elif NETFRAMEWORK
            _response.Headers[key] = value;
#endif
        }

        /// <summary>
        /// Writes content to the response body.
        /// </summary>
        /// <param name="content">The content to write.</param>
        /// <param name="contentType">The MIME type of the content. Defaults to "text/plain".</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        public async Task Write(string content, string contentType = "text/plain")
        {
#if NETCOREAPP
            _context.Response.ContentType = contentType ?? "text/plain";
            await _context.Response.WriteAsync(content);
#elif NETFRAMEWORK
            _response.ContentType = contentType ?? "text/plain";
            _response.Write(content);
#endif
        }

        /// <summary>
        /// Issues a redirect response to the specified URL.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        public void Redirect(string url)
        {
#if NETCOREAPP
            _context.Response.Redirect(url);
#elif NETFRAMEWORK
            _response.Redirect(url);
#endif
        }

        /// <summary>
        /// Sets a cookie with the specified key and value.
        /// </summary>
        /// <param name="key">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <param name="expires">Optional expiration date.</param>
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

        /// <summary>
        /// Clears the content of the current response.
        /// </summary>
        public void Clear()
        {
#if NETCOREAPP
            _context.Response.Clear();
#elif NETFRAMEWORK
            _response.Clear();
#endif
        }

        /// <summary>
        /// Terminates the response execution immediately. 
        /// Not supported in ASP.NET Core.
        /// </summary>
        public void End()
        {
#if NETCOREAPP
            // Not supported in ASP.NET Core. No-op or logging could be added here if needed.
#elif NETFRAMEWORK
            _response.End();
#endif
        }

        /// <summary>
        /// Sets the Content-Type of the response.
        /// </summary>
        /// <param name="contentType">The MIME type to set (e.g., "application/json").</param>
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