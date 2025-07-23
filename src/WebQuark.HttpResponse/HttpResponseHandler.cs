// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
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
        /// Initializes a new instance of the <see cref="HttpResponseHandler"/> class using the specified HttpContextAccessor.
        /// </summary>
        /// <param name="accessor">The HTTP context accessor.</param>
        public HttpResponseHandler(IHttpContextAccessor accessor)
        {
            _context = accessor?.HttpContext ?? throw new ArgumentNullException(nameof(accessor));
        }
#elif NETFRAMEWORK
        private readonly HttpResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseHandler"/> class using the current HttpContext.
        /// </summary>
        public HttpResponseHandler()
        {
            _response = HttpContext.Current?.Response ?? throw new InvalidOperationException("HttpResponse not available");
        }
#endif

        /// <summary>
        /// Sets the HTTP status code for the response.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set.</param>
        public void SetStatusCode(int statusCode)
        {
#if NETCOREAPP
            _context.Response.StatusCode = statusCode;
#elif NETFRAMEWORK
            _response.StatusCode = statusCode;
#endif
        }

        /// <summary>
        /// Adds or sets a header in the HTTP response.
        /// </summary>
        /// <param name="key">Header name.</param>
        /// <param name="value">Header value.</param>
        public void SetHeader(string key, string value)
        {
#if NETCOREAPP
            _context.Response.Headers[key] = value;
#elif NETFRAMEWORK
            _response.Headers[key] = value;
#endif
        }

        /// <summary>
        /// Writes content directly to the HTTP response stream.
        /// </summary>
        /// <param name="content">The content to write.</param>
        /// <param name="contentType">Optional content type (default is text/plain).</param>
        public void Write(string content, string contentType = "text/plain")
        {
#if NETCOREAPP
            _context.Response.ContentType = contentType ?? "text/plain";
            _context.Response.WriteAsync(content);
#elif NETFRAMEWORK
            _response.ContentType = contentType ?? "text/plain";
            _response.Write(content);
#endif
        }

        /// <summary>
        /// Redirects the response to the specified URL.
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
        /// Sets a cookie in the HTTP response.
        /// </summary>
        /// <param name="key">Cookie name.</param>
        /// <param name="value">Cookie value.</param>
        /// <param name="expires">Optional expiration date/time.</param>
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
        /// Clears the response buffer.
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
        /// Ends the HTTP response.
        /// </summary>
        public void End()
        {
#if NETCOREAPP
            // ASP.NET Core does not support Response.End; no-op or consider throwing
#elif NETFRAMEWORK
            _response.End();
#endif
        }

        /// <summary>
        /// Sets the content type of the response.
        /// </summary>
        /// <param name="contentType">The content type to set.</param>
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

