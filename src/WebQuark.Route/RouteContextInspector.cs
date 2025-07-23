// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using WebQuark.Core.Interfaces;

#if NETFRAMEWORK
using System.Web;
using System.Web.Routing;
#elif NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#endif

namespace WebQuark.RouteContext
{
    /// <summary>
    /// Implementation of IRouteContextInspector interface to retrieve routing and request context data.
    /// Compatible with both .NET Framework (System.Web) and .NET Core/ASP.NET Core.
    /// </summary>
    public class RouteContextInspector : IRouteContextInspector
    {
#if NETCOREAPP
        private readonly HttpContext _context;
        private readonly RouteData _routeData;
#elif NETFRAMEWORK
        private readonly HttpContext _context;
        private readonly RouteData _routeData;
#else
        private readonly HttpContext _context;
        private readonly RouteData _routeData;
#endif

#if NETCOREAPP
        public RouteContextInspector(IHttpContextAccessor httpContextAccessor)
        {
            _context = httpContextAccessor?.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _routeData = _context.GetRouteData() ?? new RouteData();
        }
#elif NETFRAMEWORK
        public RouteContextInspector()
        {
            _context = HttpContext.Current ?? throw new InvalidOperationException("HttpContext.Current is null");
            _routeData = RouteTable.Routes.GetRouteData(_context) ?? new RouteData();
        }
#else
        public RouteContextInspector()
        {
            throw new NotSupportedException("Unsupported framework");
        }
#endif

        public string GetRouteValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_routeData.Values.TryGetValue(key, out var val))
                return val?.ToString();

            return null;
        }

        public IDictionary<string, object> GetAllRouteValues()
        {
            return _routeData.Values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public string GetRequestPath()
        {
#if NETCOREAPP
            return _context.Request.Path.HasValue ? _context.Request.Path.Value : string.Empty;
#elif NETFRAMEWORK
            return _context.Request.Url?.AbsolutePath ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public string GetBasePath()
        {
#if NETCOREAPP
            return _context.Request.PathBase.HasValue ? _context.Request.PathBase.Value : string.Empty;
#elif NETFRAMEWORK
            return _context.Request.ApplicationPath ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public string GetRequestScheme()
        {
#if NETCOREAPP
            return _context.Request.Scheme;
#elif NETFRAMEWORK
            return _context.Request.Url?.Scheme ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public string GetHost()
        {
#if NETCOREAPP
            return _context.Request.Host.HasValue ? _context.Request.Host.Value : string.Empty;
#elif NETFRAMEWORK
            return _context.Request.Url?.Host ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public int? GetPort()
        {
#if NETCOREAPP
            return _context.Request.Host.Port;
#elif NETFRAMEWORK
            return _context.Request.Url?.Port;
#else
            return null;
#endif
        }

        public string GetFullUrl()
        {
#if NETCOREAPP
            var req = _context.Request;
            return $"{req.Scheme}://{req.Host}{req.PathBase}{req.Path}{req.QueryString}";
#elif NETFRAMEWORK
            return _context.Request.Url?.ToString() ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public string GetControllerName()
        {
            return GetRouteValue("controller");
        }

        public string GetActionName()
        {
            return GetRouteValue("action");
        }

        public string GetAreaName()
        {
            return GetRouteValue("area");
        }
    }
}