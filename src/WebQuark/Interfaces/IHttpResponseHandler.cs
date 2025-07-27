// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.Threading.Tasks;

namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Defines a platform-agnostic interface for manipulating HTTP response data,
    /// including status codes, headers, cookies, and redirections.
    /// </summary>
    public interface IHttpResponseHandler
    {
        void SetStatusCode(int statusCode);
        void SetHeader(string key, string value);
        void SetCookie(string key, string value, DateTime? expires = null);
        void Redirect(string url);
    }
}