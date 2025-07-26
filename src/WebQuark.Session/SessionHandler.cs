// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Text.Json;
using WebQuark.Core.Interfaces;
using WebQuark.Session.Utilities;

#if NETFRAMEWORK
using System.Web;
using System.Web.SessionState;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
#endif

namespace WebQuark.Session
{
    /// <summary>
    /// Unified session handler compatible with both ASP.NET Core and ASP.NET Framework.
    /// </summary>
    public class SessionHandler : ISessionHandler
    {
#if NETCOREAPP
        private readonly ISession _session;

        public SessionHandler(IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor?.HttpContext?.Session == null)
                throw new ArgumentNullException(nameof(contextAccessor), "HttpContext or Session is null");

            _session = contextAccessor.HttpContext.Session;
        }
#elif NETFRAMEWORK
        private readonly HttpSessionState _session;

        public SessionHandler()
        {
            _session = HttpContext.Current?.Session ?? throw new InvalidOperationException("Session is not available.");
        }
#endif

        public void SetString(string key, string value)
        {
#if NETCOREAPP
            _session.SetString(key, value);
#elif NETFRAMEWORK
            _session[key] = value;
#endif
        }

        public string GetString(string key)
        {
#if NETCOREAPP
            return _session.GetString(key);
#elif NETFRAMEWORK
            return _session[key]?.ToString();
#else
            return null;
#endif
        }

        public void Set<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            SetString(key, json);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            var json = GetString(key);
            if (string.IsNullOrWhiteSpace(json)) return defaultValue;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool HasKey(string key)
        {
#if NETCOREAPP
            return _session.GetString(key) != null;
#elif NETFRAMEWORK
            return _session[key] != null;
#else
            return false;
#endif
        }

        public void Remove(string key)
        {
#if NETCOREAPP
            _session.Remove(key);
#elif NETFRAMEWORK
            _session.Remove(key);
#endif
        }

        public void Clear()
        {
#if NETCOREAPP
            _session.Clear();
#elif NETFRAMEWORK
            _session.Clear();
#endif
        }

        public void SetEncrypted<T>(string key, T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var json = JsonSerializer.Serialize(value);
            var encrypted = EncryptionUtils.Encrypt(key, json);
            SetString(key, encrypted);
        }

        public T GetEncrypted<T>(string key, T defaultValue = default)
        {
            var encrypted = GetString(key);
            if (string.IsNullOrEmpty(encrypted)) return defaultValue;

            try
            {
                var json = EncryptionUtils.Decrypt(key, encrypted);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}