// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Defines a cross-platform abstraction for working with HTTP session state,
    /// supporting both plain and encrypted data storage and retrieval.
    /// </summary>
    public interface ISessionHandler
    {
        /// <summary>
        /// Stores a string value in the session state with the specified key.
        /// </summary>
        /// <param name="key">The session key.</param>
        /// <param name="value">The string value to store.</param>
        void SetString(string key, string value);

        /// <summary>
        /// Retrieves a string value from the session state by key.
        /// </summary>
        /// <param name="key">The session key.</param>
        /// <returns>The string value associated with the key, or null if not found.</returns>
        string GetString(string key);

        /// <summary>
        /// Stores a typed value in the session state with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="value">The value to store.</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Retrieves a typed value from the session state by key.
        /// Returns the default value if the key is not found or conversion fails.
        /// </summary>
        /// <typeparam name="T">The expected type of the value.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="defaultValue">The default value to return if not found.</param>
        /// <returns>The value associated with the key, or the default value.</returns>
        T Get<T>(string key, T defaultValue = default);

        /// <summary>
        /// Checks whether the session state contains the specified key.
        /// </summary>
        /// <param name="key">The session key.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        bool HasKey(string key);

        /// <summary>
        /// Removes the specified key and its value from the session state.
        /// </summary>
        /// <param name="key">The session key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Clears all keys and values from the session state.
        /// </summary>
        void Clear();

        /// <summary>
        /// Stores an encrypted typed value in the session state with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to encrypt and store.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="value">The value to encrypt and store.</param>
        void SetEncrypted<T>(string key, T value);

        /// <summary>
        /// Retrieves an encrypted typed value from the session state by key.
        /// Returns the default value if the key is not found or decryption/conversion fails.
        /// </summary>
        /// <typeparam name="T">The expected type of the decrypted value.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="defaultValue">The default value to return if not found or invalid.</param>
        /// <returns>The decrypted value associated with the key, or the default value.</returns>
        T GetEncrypted<T>(string key, T defaultValue = default);

        /// <summary>
        /// Attempts to retrieve a typed value from the session. Returns true if successful.
        /// </summary>
        /// <typeparam name="T">The expected type of the value.</typeparam>
        /// <param name="key">The session key.</param>
        /// <param name="value">The output value if present and valid.</param>
        /// <returns>True if the key exists and conversion succeeds; otherwise, false.</returns>
        bool TryGet<T>(string key, out T value);
    }
}