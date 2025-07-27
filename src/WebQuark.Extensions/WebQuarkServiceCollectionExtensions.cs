// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.Extensions.DependencyInjection;
using WebQuark.Core.Interfaces;
using WebQuark.QueryString;
using WebQuark.HttpRequest;
using WebQuark.Session;
using WebQuark.HttpResponse;

namespace WebQuark.Extensions
{
    /// <summary>
    /// Extension methods for registering WebQuark core services in the application's dependency injection container.
    /// </summary>
    public static class WebQuarkServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all core WebQuark services required to enable unified HTTP abstraction across platforms.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddWebQuark(this IServiceCollection services)
        {
            services.AddScoped<IHttpRequestInspector, HttpRequestInspector>();
            services.AddScoped<IHttpResponseHandler, HttpResponseHandler>();
            services.AddScoped<IRequestQueryHandler, QueryStringHandler>();
            services.AddScoped<ISessionHandler, SessionHandler>();

            return services;
        }

        /// <summary>
        /// Registers the IHttpRequestInspector service.
        /// </summary>
        public static IServiceCollection AddWebQuarkHttpRequestInspector(this IServiceCollection services)
        {
            services.AddScoped<IHttpRequestInspector, HttpRequestInspector>();
            return services;
        }

        /// <summary>
        /// Registers the IHttpResponseHandler service.
        /// </summary>
        public static IServiceCollection AddWebQuarkHttpResponseHandler(this IServiceCollection services)
        {
            services.AddScoped<IHttpResponseHandler, HttpResponseHandler>();
            return services;
        }

        /// <summary>
        /// Registers the IRequestQueryHandler service.
        /// </summary>
        public static IServiceCollection AddWebQuarkRequestQueryHandler(this IServiceCollection services)
        {
            services.AddScoped<IRequestQueryHandler, QueryStringHandler>();
            return services;
        }

        /// <summary>
        /// Registers the ISessionHandler service.
        /// </summary>
        public static IServiceCollection AddWebQuarkSessionHandler(this IServiceCollection services)
        {
            services.AddScoped<ISessionHandler, SessionHandler>();
            return services;
        }
    }
}