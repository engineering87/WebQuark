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
#endif

namespace WebQuark.Session
{
    /// <summary>
    /// Provides a unified API for managing session data across both
    /// ASP.NET Framework and ASP.NET Core environments.
    /// </summary>
    public class SessionHandler : ISessionHandler
    {
#if NETCOREAPP
        private readonly ISession _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionHandler"/> class for ASP.NET Core.
        /// </summary>
        /// <param name="contextAccessor">Provides access to the current HTTP context.</param>
        public SessionHandler(IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor?.HttpContext?.Session == null)
                throw new ArgumentNullException(nameof(contextAccessor), "HttpContext or Session is null.");

            _session = contextAccessor.HttpContext.Session;
        }
#elif NETFRAMEWORK
        private readonly HttpSessionState _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionHandler"/> class for .NET Framework.
        /// </summary>
        public SessionHandler()
        {
            _session = HttpContext.Current?.Session ?? throw new InvalidOperationException("Session is not available.");
        }
#endif

        /// <summary>
        /// Stores a string value in session storage under the specified key.
        /// </summary>
        public void SetString(string key, string value)
        {
#if NETCOREAPP
            _session.SetString(key, value);
#elif NETFRAMEWORK
            _session[key] = value;
#endif
        }

        /// <summary>
        /// Retrieves a string value from session storage by key.
        /// </summary>
        /// <param name="key">The session key.</param>
        /// <returns>The string value, or null if not found.</returns>
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

        /// <summary>
        /// Stores a serializable object in session storage after converting it to JSON.
        /// </summary>
        /// <typeparam name="T">The type of the object to store.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="value">The value to store.</param>
        public void Set<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            SetString(key, json);
        }

        /// <summary>
        /// Retrieves a deserialized object of type <typeparamref name="T"/> from session storage.
        /// Returns the provided default value if the key is not found or deserialization fails.
        /// </summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="defaultValue">Optional default value to return if retrieval fails.</param>
        /// <returns>The deserialized object, or the default value.</returns>
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

        /// <summary>
        /// Checks whether the specified key exists in session storage.
        /// </summary>
        /// <param name="key">The session key to check.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Removes the specified key and its value from session storage.
        /// </summary>
        /// <param name="key">The session key to remove.</param>
        public void Remove(string key)
        {
#if NETCOREAPP
            _session.Remove(key);
#elif NETFRAMEWORK
            _session.Remove(key);
#endif
        }

        /// <summary>
        /// Clears all session data.
        /// </summary>
        public void Clear()
        {
#if NETCOREAPP
            _session.Clear();
#elif NETFRAMEWORK
            _session.Clear();
#endif
        }

        /// <summary>
        /// Encrypts a serializable object and stores it in session storage as a string.
        /// </summary>
        /// <typeparam name="T">The type of the object to store.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="value">The object to encrypt and store.</param>
        public void SetEncrypted<T>(string key, T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var json = JsonSerializer.Serialize(value);
            var encrypted = EncryptionUtils.Encrypt(key, json);
            SetString(key, encrypted);
        }

        /// <summary>
        /// Retrieves and decrypts a string from session storage,
        /// attempting to deserialize it into an object of type <typeparamref name="T"/>.
        /// Returns the default value if decryption or deserialization fails.
        /// </summary>
        /// <typeparam name="T">The expected object type.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="defaultValue">Optional default value to return if operation fails.</param>
        /// <returns>The decrypted and deserialized object, or the default value.</returns>
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