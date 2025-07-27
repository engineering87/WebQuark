// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WebQuark.Core.Interfaces;

#if NETFRAMEWORK
using System.Web;
using HttpContextBase = System.Web.HttpContext;
using HttpRequestBase = System.Web.HttpRequest;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
#endif

namespace WebQuark.HttpRequest
{
    /// <summary>
    /// Provides a unified interface to inspect HTTP request data across multiple .NET frameworks.
    /// </summary>
    public class HttpRequestInspector : IHttpRequestInspector
    {
#if NETCOREAPP
        private readonly HttpContext _context;
        public HttpRequestInspector(IHttpContextAccessor contextAccessor)
        {
            _context = contextAccessor?.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor));
        }
#elif NETFRAMEWORK
        private readonly HttpRequestBase _request;
        public HttpRequestInspector()
        {
            _request = HttpContextBase.Current?.Request ?? throw new InvalidOperationException("HttpRequest not available");
        }
#endif

        /// <summary>
        /// Returns the HTTP method of the current request (e.g., GET, POST).
        /// </summary>
        public string GetHttpMethod()
        {
#if NETCOREAPP
            return _context.Request.Method;
#elif NETFRAMEWORK
            return _request.HttpMethod;
#else
            return null;
#endif
        }

        /// <summary>
        /// Retrieves the value of a specific HTTP header.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <returns>The header value, or null if not present.</returns>
        public string GetHeader(string key)
        {
#if NETCOREAPP
            return _context.Request.Headers.TryGetValue(key, out var values) ? values.ToString() : null;
#elif NETFRAMEWORK
            return _request.Headers[key];
#else
            return null;
#endif
        }

        /// <summary>
        /// Determines whether a specific HTTP header is present.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <returns>True if the header exists; otherwise, false.</returns>
        public bool HasHeader(string key)
        {
#if NETCOREAPP
            return _context.Request.Headers.ContainsKey(key);
#elif NETFRAMEWORK
            return _request.Headers[key] != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns all HTTP headers as a dictionary.
        /// </summary>
        /// <returns>A dictionary of header key-value pairs.</returns>
        public IDictionary<string, string> GetAllHeaders()
        {
#if NETCOREAPP
            return _context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
#elif NETFRAMEWORK
            return _request.Headers.AllKeys.ToDictionary(k => k, k => _request.Headers[k]);
#else
            return new Dictionary<string, string>();
#endif
        }

        /// <summary>
        /// Retrieves the value of a specific query string parameter.
        /// </summary>
        /// <param name="key">The query string key.</param>
        /// <param name="defaultValue">The default value if the key is not present.</param>
        /// <returns>The value of the query parameter, or the default value.</returns>
        public string GetQueryString(string key, string defaultValue = null)
        {
#if NETCOREAPP
            return _context.Request.Query.TryGetValue(key, out var values) ? values.ToString() : defaultValue;
#elif NETFRAMEWORK
            var val = _request.QueryString[key];
            return val ?? defaultValue;
#else
            return defaultValue;
#endif
        }

        /// <summary>
        /// Returns all query string parameters as a dictionary.
        /// </summary>
        /// <returns>A dictionary of query string key-value pairs.</returns>
        public IDictionary<string, string> GetAllQueryStrings()
        {
#if NETCOREAPP
            return _context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
#elif NETFRAMEWORK
            return _request.QueryString.AllKeys.ToDictionary(k => k, k => _request.QueryString[k]);
#else
            return new Dictionary<string, string>();
#endif
        }

        /// <summary>
        /// Retrieves the value of a specific cookie.
        /// </summary>
        /// <param name="key">The name of the cookie.</param>
        /// <returns>The cookie value, or null if not found.</returns>
        public string GetCookie(string key)
        {
#if NETCOREAPP
            return _context.Request.Cookies.TryGetValue(key, out var value) ? value : null;
#elif NETFRAMEWORK
            return _request.Cookies[key]?.Value;
#else
            return null;
#endif
        }

        /// <summary>
        /// Determines whether a specific cookie is present.
        /// </summary>
        /// <param name="key">The name of the cookie.</param>
        /// <returns>True if the cookie exists; otherwise, false.</returns>
        public bool HasCookie(string key)
        {
#if NETCOREAPP
            return _context.Request.Cookies.ContainsKey(key);
#elif NETFRAMEWORK
            return _request.Cookies[key] != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns all cookies as a dictionary.
        /// </summary>
        /// <returns>A dictionary of cookie key-value pairs.</returns>
        public IDictionary<string, string> GetAllCookies()
        {
#if NETCOREAPP
            return _context.Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
#elif NETFRAMEWORK
            return _request.Cookies.AllKeys.ToDictionary(k => k, k => _request.Cookies[k]?.Value);
#else
            return new Dictionary<string, string>();
#endif
        }

        /// <summary>
        /// Reads the raw request body as a string.
        /// </summary>
        /// <returns>The body content as a string.</returns>
        public string GetBodyAsString()
        {
#if NETCOREAPP
            var request = _context.Request;
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = reader.ReadToEnd();
            request.Body.Position = 0;
            return body;
#elif NETFRAMEWORK
            var input = _request.InputStream;
            input.Position = 0;
            using (var reader = new StreamReader(input))
            {
                return reader.ReadToEnd();
            }
#else
            return null;
#endif
        }

        /// <summary>
        /// Deserializes the request body into a JSON object of type T.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <returns>An object of type T, or default if deserialization fails.</returns>
        public T GetBodyAsJson<T>()
        {
            var body = GetBodyAsString();
            if (string.IsNullOrWhiteSpace(body))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(body);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Retrieves the User-Agent string from the request headers.
        /// </summary>
        /// <returns>The User-Agent value.</returns>
        public string GetUserAgent()
        {
#if NETCOREAPP
            return GetHeader("User-Agent");
#elif NETFRAMEWORK
            return _request.UserAgent;
#else
            return null;
#endif
        }

        /// <summary>
        /// Attempts to determine the client IP address, checking headers and connection metadata.
        /// </summary>
        /// <returns>The client's IP address as a string.</returns>
        public string GetClientIpAddress()
        {
#if NETCOREAPP
            var ip = _context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip)) return ip;

            return GetHeader("X-Forwarded-For")?.Split(',')?.FirstOrDefault()?.Trim();
#elif NETFRAMEWORK
            var ipAddr = _request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddr))
                return ipAddr.Split(',')?.FirstOrDefault()?.Trim();

            return _request.UserHostAddress;
#else
            return null;
#endif
        }

        /// <summary>
        /// Determines whether the request was made using AJAX.
        /// </summary>
        /// <returns>True if the request is an AJAX call; otherwise, false.</returns>
        public bool IsAjaxRequest()
        {
            var val = GetHeader("X-Requested-With");
            return string.Equals(val, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}