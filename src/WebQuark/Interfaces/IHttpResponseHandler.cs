// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;

namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Defines a platform-agnostic interface for manipulating HTTP response data,
    /// including status codes, headers, cookies, and redirections.
    /// </summary>
    public interface IHttpResponseHandler
    {
        /// <summary>
        /// Sets the HTTP status code of the response.
        /// </summary>
        /// <param name="statusCode">The status code to set (e.g., 200, 404).</param>
        void SetStatusCode(int statusCode);

        /// <summary>
        /// Sets or updates a header in the HTTP response.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header value.</param>
        void SetHeader(string key, string value);

        /// <summary>
        /// Sets a cookie in the HTTP response.
        /// </summary>
        /// <param name="key">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <param name="expires">Optional expiration date for the cookie.</param>
        void SetCookie(string key, string value, DateTime? expires = null);

        /// <summary>
        /// Issues a redirect to the specified URL.
        /// </summary>
        /// <param name="url">The URL to redirect the client to.</param>
        void Redirect(string url);
    }
}