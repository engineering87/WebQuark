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
#else
        // Stub for netstandard2.0 or unsupported environments
        public HttpRequestInspector() => throw new PlatformNotSupportedException("HttpRequestInspector is not supported on this platform.");
#endif

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
            using var reader = new StreamReader(input);
            return reader.ReadToEnd();
#else
            return null;
#endif
        }

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

        public bool IsAjaxRequest()
        {
            var val = GetHeader("X-Requested-With");
            return string.Equals(val, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}