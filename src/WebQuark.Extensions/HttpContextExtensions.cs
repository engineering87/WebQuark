// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebQuark.Core.Interfaces;

namespace WebQuark.Extensions
{
    /// <summary>
    /// Provides extension methods on HttpContext to access WebQuark core services
    /// via dependency injection in ASP.NET Core applications.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the registered implementation of <see cref="IHttpRequestInspector"/> from the current HTTP context.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>The <see cref="IHttpRequestInspector"/> implementation.</returns>
        public static IHttpRequestInspector RequestInspector(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IHttpRequestInspector>();
        }

        /// <summary>
        /// Gets the registered implementation of <see cref="IHttpResponseHandler"/> from the current HTTP context.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>The <see cref="IHttpResponseHandler"/> implementation.</returns>
        public static IHttpResponseHandler ResponseHandler(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IHttpResponseHandler>();
        }

        /// <summary>
        /// Gets the registered implementation of <see cref="IRequestQueryHandler"/> from the current HTTP context.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>The <see cref="IRequestQueryHandler"/> implementation.</returns>
        public static IRequestQueryHandler QueryHandler(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IRequestQueryHandler>();
        }

        /// <summary>
        /// Gets the registered implementation of <see cref="ISessionHandler"/> from the current HTTP context.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/> instance.</param>
        /// <returns>The <see cref="ISessionHandler"/> implementation.</returns>
        public static ISessionHandler SessionHandler(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<ISessionHandler>();
        }
    }
}