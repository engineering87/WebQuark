// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System.Collections.Generic;

namespace WebQuark.Core.Interfaces
{
    /// <summary>
    /// Interface to inspect route data and request context information.
    /// Supports retrieval of route parameters, request paths, host, scheme, and full URL.
    /// </summary>
    public interface IRouteContextInspector
    {
        string GetRouteValue(string key);
        IDictionary<string, object> GetAllRouteValues();
        string GetRequestPath();
        string GetBasePath();
        string GetRequestScheme();
        string GetHost();
        int? GetPort();
        string GetFullUrl();
        string GetControllerName();
        string GetActionName();
        string GetAreaName();
    }
}
