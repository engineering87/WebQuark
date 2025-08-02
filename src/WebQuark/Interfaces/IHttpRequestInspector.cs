// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System.Collections.Generic;

namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Defines a unified interface for inspecting HTTP requests, providing
    /// access to HTTP method, headers, query strings, cookies, body content,
    /// and client-related metadata.
    /// </summary>
    public interface IHttpRequestInspector
    {
        /// <summary>
        /// Gets the HTTP method (e.g., GET, POST) of the current request.
        /// </summary>
        /// <returns>The HTTP method as a string.</returns>
        string GetHttpMethod();

        /// <summary>
        /// Retrieves the value of a specific header from the HTTP request.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <returns>The header value, or null if not present.</returns>
        string GetHeader(string key);

        /// <summary>
        /// Checks whether the specified header exists in the request.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <returns>True if the header exists; otherwise, false.</returns>
        bool HasHeader(string key);

        /// <summary>
        /// Retrieves all headers from the HTTP request as a dictionary.
        /// </summary>
        /// <returns>A dictionary of header names and their values.</returns>
        IDictionary<string, string> GetAllHeaders();

        /// <summary>
        /// Retrieves the value of a specific query string parameter.
        /// </summary>
        /// <param name="key">The query string parameter name.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not present.</param>
        /// <returns>The query string value or the default value.</returns>
        string GetQueryString(string key, string defaultValue = null);

        /// <summary>
        /// Retrieves all query string parameters as a dictionary.
        /// </summary>
        /// <returns>A dictionary of query string keys and values.</returns>
        IDictionary<string, string> GetAllQueryStrings();

        /// <summary>
        /// Retrieves the value of a specific cookie.
        /// </summary>
        /// <param name="key">The cookie name.</param>
        /// <returns>The cookie value, or null if not present.</returns>
        string GetCookie(string key);

        /// <summary>
        /// Checks whether a specific cookie exists.
        /// </summary>
        /// <param name="key">The cookie name.</param>
        /// <returns>True if the cookie exists; otherwise, false.</returns>
        bool HasCookie(string key);

        /// <summary>
        /// Retrieves all cookies as a dictionary.
        /// </summary>
        /// <returns>A dictionary of cookie names and their values.</returns>
        IDictionary<string, string> GetAllCookies();

        /// <summary>
        /// Reads the body content of the HTTP request as a string.
        /// </summary>
        /// <returns>The body content as a string.</returns>
        string GetBodyAsString();

        /// <summary>
        /// Deserializes the HTTP request body content into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize into.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> representing the deserialized body content.</returns>
        T GetBodyAsJson<T>();

        /// <summary>
        /// Retrieves the User-Agent string from the HTTP request headers.
        /// </summary>
        /// <returns>The User-Agent string.</returns>
        string GetUserAgent();

        /// <summary>
        /// Retrieves the client IP address from the request context.
        /// </summary>
        /// <returns>The client IP address as a string.</returns>
        string GetClientIpAddress();

        /// <summary>
        /// Determines whether the HTTP request was made via AJAX.
        /// </summary>
        /// <returns>True if the request is an AJAX request; otherwise, false.</returns>
        bool IsAjaxRequest();

        /// <summary>
        /// Retrieves the Content-Type header of the HTTP request.
        /// </summary>
        /// <returns>The Content-Type string, or null if not present.</returns>
        string GetContentType();
    }
}