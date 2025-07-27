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
        string GetHttpMethod();
        string GetHeader(string key);
        bool HasHeader(string key);
        IDictionary<string, string> GetAllHeaders();
        string GetQueryString(string key, string defaultValue = null);
        IDictionary<string, string> GetAllQueryStrings();
        string GetCookie(string key);
        bool HasCookie(string key);
        IDictionary<string, string> GetAllCookies();
        string GetBodyAsString();
        T GetBodyAsJson<T>();
        string GetUserAgent();
        string GetClientIpAddress();
        bool IsAjaxRequest();
    }
}
