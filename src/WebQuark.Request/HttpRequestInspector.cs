// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Collections.Generic;
using System.Text.Json;
using WebQuark.Core.Interfaces;

#if NETFRAMEWORK
using System.Web;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace WebQuark.HttpRequest
{
    /// <summary>
    /// Provides a unified interface to inspect HTTP request data across multiple .NET frameworks.
    /// Supports accessing HTTP method, headers, query strings, cookies, request body, and client info.
    /// Compatible with both ASP.NET Framework and ASP.NET Core.
    /// </summary>
    public class HttpRequestInspector : IHttpRequestInspector
    {
#if NETCOREAPP
        private readonly HttpContext _context;

        /// <summary>
        /// Constructor for ASP.NET Core, requires IHttpContextAccessor for access to HttpContext.
        /// </summary>
        public HttpRequestInspector(IHttpContextAccessor contextAccessor)
        {
            _context = contextAccessor?.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor));
        }
#elif NETFRAMEWORK
        private readonly HttpRequest _request;

        /// <summary>
        /// Constructor for ASP.NET Framework, gets HttpRequest from current HttpContext.
        /// </summary>
        public HttpRequestInspector()
        {
            _request = HttpContext.Current?.Request ?? throw new InvalidOperationException("HttpRequest not available");
        }
#endif

        /// <summary>
        /// Gets the HTTP method of the current request (e.g. GET, POST).
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
        /// Retrieves the value of a specific HTTP header by key.
        /// </summary>
        public string GetHeader(string key)
        {
#if NETCOREAPP
            if (_context.Request.Headers.TryGetValue(key, out var values))
                return values.ToString();
            return null;
#elif NETFRAMEWORK
            return _request.Headers[key];
#else
            return null;
#endif
        }

        /// <summary>
        /// Checks if a specific HTTP header is present in the request.
        /// </summary>
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
        /// Gets a query string parameter value by key, or returns a default value if not present.
        /// </summary>
        public string GetQueryString(string key, string defaultValue = null)
        {
#if NETCOREAPP
            if (_context.Request.Query.TryGetValue(key, out var values))
                return values.ToString();
            return defaultValue;
#elif NETFRAMEWORK
            var val = _request.QueryString[key];
            return val ?? defaultValue;
#else
            return defaultValue;
#endif
        }

        /// <summary>
        /// Retrieves all query string parameters as a dictionary.
        /// </summary>
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
        /// Gets the value of a specific cookie by key.
        /// </summary>
        public string GetCookie(string key)
        {
#if NETCOREAPP
            if (_context.Request.Cookies.TryGetValue(key, out var cookie))
                return cookie;
            return null;
#elif NETFRAMEWORK
            return _request.Cookies[key]?.Value;
#else
            return null;
#endif
        }

        /// <summary>
        /// Checks if a specific cookie is present in the request.
        /// </summary>
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
        public IDictionary<string, string> GetAllCookies()
        {
#if NETCOREAPP
            return _context.Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
#elif NETFRAMEWORK
            var dict = new Dictionary<string, string>();
            foreach (string key in _request.Cookies.AllKeys)
            {
                dict[key] = _request.Cookies[key].Value;
            }
            return dict;
#else
            return new Dictionary<string, string>();
#endif
        }

        /// <summary>
        /// Reads the raw HTTP request body as a string.
        /// </summary>
        public string GetBodyAsString()
        {
#if NETCOREAPP
            var req = _context.Request;
            req.EnableBuffering();

            req.Body.Position = 0;
            using (var reader = new StreamReader(req.Body, leaveOpen: true))
            {
                string body = reader.ReadToEnd();
                req.Body.Position = 0;
                return body;
            }
#elif NETFRAMEWORK
            var inputStream = _request.InputStream;
            inputStream.Position = 0;
            using (var reader = new StreamReader(inputStream))
            {
                return reader.ReadToEnd();
            }
#else
            return null;
#endif
        }

        /// <summary>
        /// Deserializes the HTTP request body JSON into the specified type T.
        /// </summary>
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
        /// Retrieves the client IP address of the request, checking proxy headers if necessary.
        /// </summary>
        public string GetClientIpAddress()
        {
#if NETCOREAPP
            var ip = _context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip)) return ip;

            // fallback from headers (proxy/load balancer)
            ip = GetHeader("X-Forwarded-For");
            return ip?.Split(',').FirstOrDefault()?.Trim();
#elif NETFRAMEWORK
            var ipAddr = _request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddr))
                return ipAddr.Split(',').FirstOrDefault()?.Trim();

            return _request.UserHostAddress;
#else
            return null;
#endif
        }

        /// <summary>
        /// Checks if the request was made via AJAX by inspecting the "X-Requested-With" header.
        /// </summary>
        public bool IsAjaxRequest()
        {
            var headerVal = GetHeader("X-Requested-With");
            return string.Equals(headerVal, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}