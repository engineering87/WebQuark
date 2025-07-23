// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using WebQuark.Core.Interfaces;
#if NETFRAMEWORK
using System.Web;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace WebQuark.QueryString
{
    /// <summary>
    /// Handles reading and writing of HTTP query string parameters, supporting .NET Framework and ASP.NET Core.
    /// </summary>
    public class QueryStringHandler : IRequestQueryHandler
    {
        private readonly NameValueCollection _queryCollection;

#if NETCOREAPP
        private static IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Configures the IHttpContextAccessor required for ASP.NET Core to access the current HTTP context.
        /// </summary>
        /// <param name="accessor">The IHttpContextAccessor to be used.</param>
        public static void ConfigureHttpContextAccessor(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }
#endif

        /// <summary>
        /// Initializes a new instance of the QueryStringHandler class using the current HTTP request's query string.
        /// </summary>
        public QueryStringHandler()
        {
            string rawQuery = string.Empty;

#if NETFRAMEWORK
        rawQuery = HttpContext.Current?.Request?.Url?.Query ?? string.Empty;
#elif NETCOREAPP
        rawQuery = _httpContextAccessor?.HttpContext?.Request?.QueryString.Value ?? string.Empty;
#endif

            _queryCollection = HttpUtility.ParseQueryString(rawQuery);
        }

        /// <summary>
        /// Retrieves the string value associated with the given key. Returns a default if the key is missing.
        /// </summary>
        public string Get(string key, string defaultValue = null)
        {
            return _queryCollection[key] ?? defaultValue;
        }

        /// <summary>
        /// Sets the string value for the given key.
        /// </summary>
        public void Set(string key, string value)
        {
            _queryCollection[key] = value;
        }

        /// <summary>
        /// Checks whether the given key exists in the query string.
        /// </summary>
        public bool HasKey(string key)
        {
            return _queryCollection[key] != null;
        }

        /// <summary>
        /// Removes the specified key and its value from the query string.
        /// </summary>
        public void Remove(string key)
        {
            _queryCollection.Remove(key);
        }

        /// <summary>
        /// Retrieves a typed value from the query string, attempting conversion from string.
        /// Returns the default if the key is missing or conversion fails.
        /// </summary>
        public T Get<T>(string key, T defaultValue = default)
        {
            var strVal = Get(key);
            if (string.IsNullOrWhiteSpace(strVal)) 
                return defaultValue;

            try
            {
                var targetType = typeof(T);

                if (targetType.IsEnum)
                    return (T)Enum.Parse(targetType, strVal, ignoreCase: true);

                if (targetType == typeof(Guid))
                    return (T)(object)Guid.Parse(strVal);

                if (targetType == typeof(DateTime))
                    return (T)(object)DateTime.Parse(strVal);

                return (T)Convert.ChangeType(strVal, targetType);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a typed value into the query string after converting it to string.
        /// </summary>
        public void Set<T>(string key, T value)
        {
            string strVal = Convert.ToString(value);
            Set(key, strVal);
        }

        /// <summary>
        /// Returns all the keys present in the query string.
        /// </summary>
        public IEnumerable<string> AllKeys()
        {
            return _queryCollection.AllKeys.Where(k => k != null);
        }

        /// <summary>
        /// Returns the query string as a dictionary of key-value pairs.
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            return _queryCollection.AllKeys
                .Where(k => k != null)
                .ToDictionary(k => k, k => _queryCollection[k]);
        }

        /// <summary>
        /// Returns the raw query string reconstructed from the internal key-value pairs.
        /// </summary>
        public string ToQueryString()
        {
            return string.Join("&", _queryCollection.AllKeys
                .Where(k => k != null)
                .Select(k => $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(_queryCollection[k])}"));
        }

        /// <summary>
        /// Returns a URL-encoded version of the raw query string.
        /// </summary>
        public string ToEncodedString()
        {
            var raw = ToString();
            return HttpUtility.UrlEncode(raw);
        }
    }
}
