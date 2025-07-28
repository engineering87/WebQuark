// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System.Collections.Generic;

namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Provides an abstraction for reading, modifying, and serializing query string parameters
    /// from HTTP requests in a consistent and platform-agnostic way.
    /// </summary>
    public interface IRequestQueryHandler
    {
        /// <summary>
        /// Retrieves the string value associated with the specified key from the query string.
        /// Returns the specified default value if the key does not exist.
        /// </summary>
        /// <param name="key">The query string key.</param>
        /// <param name="defaultValue">The value to return if the key is not found.</param>
        /// <returns>The string value associated with the key or the default value.</returns>
        string Get(string key, string defaultValue = null);

        /// <summary>
        /// Sets or updates the value for the specified key in the query string.
        /// </summary>
        /// <param name="key">The query string key.</param>
        /// <param name="value">The value to set.</param>
        void Set(string key, string value);

        /// <summary>
        /// Determines whether the specified key exists in the query string.
        /// </summary>
        /// <param name="key">The query string key.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        bool HasKey(string key);

        /// <summary>
        /// Removes the specified key and its associated value from the query string.
        /// </summary>
        /// <param name="key">The query string key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Retrieves a typed value associated with the specified key from the query string.
        /// Returns the default value if the key is missing or conversion fails.
        /// </summary>
        /// <typeparam name="T">The target type to convert the value to.</typeparam>
        /// <param name="key">The query string key.</param>
        /// <param name="defaultValue">The default value to return if the key is missing or invalid.</param>
        /// <returns>The converted value of type T or the default value.</returns>
        T Get<T>(string key, T defaultValue = default);

        /// <summary>
        /// Sets a typed value for the specified key in the query string after converting it to a string.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="key">The query string key.</param>
        /// <param name="value">The value to set.</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Retrieves all keys currently present in the query string.
        /// </summary>
        /// <returns>An enumerable collection of all keys.</returns>
        IEnumerable<string> AllKeys();

        /// <summary>
        /// Converts the entire query string into a dictionary of key-value pairs.
        /// </summary>
        /// <returns>A dictionary containing all query string parameters.</returns>
        Dictionary<string, string> ToDictionary();

        /// <summary>
        /// Serializes the query string parameters back into a raw query string format.
        /// </summary>
        /// <returns>The reconstructed query string.</returns>
        string ToQueryString();

        /// <summary>
        /// Returns a URL-encoded version of the query string.
        /// </summary>
        /// <returns>The URL-encoded query string.</returns>
        string ToEncodedString();

        /// <summary>
        /// Determines whether the query string contains any parameters.
        /// </summary>
        /// <returns>True if empty; otherwise, false.</returns>
        bool IsEmpty();

        /// <summary>
        /// Adds multiple key-value pairs to the query string, overwriting existing keys if necessary.
        /// </summary>
        /// <param name="items">A dictionary of keys and values to add.</param>
        void AddRange(Dictionary<string, string> items);
    }
}