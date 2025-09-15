// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under the MIT license (see LICENSE.txt for details)
#if ASPNETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebQuark.Cors.Internal;
using WebQuark.Cors.Options;

namespace WebQuark.Cors.Extensions
{
    /// <summary>
    /// Extension methods to register and apply WebQuark CORS policies (ASP.NET Core).
    /// </summary>
    public static class WebQuarkCorsExtensions
    {
        /// <summary>
        /// Registers WebQuark CORS with two named policies: a public read-only policy and a trusted allowlist policy.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional configuration callback for <see cref="WebQuarkCorsOptions"/>.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// <code>
        /// // Program.cs (ASP.NET Core)
        /// builder.Services.AddWebQuarkCors(o =>
        /// {
        ///     o.AllowedOrigins   = new[] { "https://app.example.com", "https://localhost:5173" };
        ///     o.AllowCredentials = true;   // disallowed with "*"
        ///     o.PreflightMaxAge  = TimeSpan.FromHours(2);
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddWebQuarkCors(
            this IServiceCollection services,
            Action<WebQuarkCorsOptions> configure = null)
        {
            services.AddOptions<WebQuarkCorsOptions>();
            if (configure != null) services.Configure(configure);

            services.AddCors();
            services.AddSingleton<IConfigureOptions<CorsOptions>, ConfigureCorsOptions>();
            return services;
        }

        /// <summary>
        /// Applies a default CORS policy to the pipeline. If you need per-endpoint control, omit this and use RequireCors.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="defaultPolicy">Optional policy name to apply (e.g., "cors-trusted" or "cors-public").</param>
        /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
        /// <example>
        /// <code>
        /// // Apply a default policy to the whole app:
        /// app.UseWebQuarkCors("cors-trusted");
        /// </code>
        /// <code>
        /// // Per-endpoint usage (Minimal APIs):
        /// app.MapGet("/ping", () => "ok").RequireCors("cors-public");
        /// </code>
        /// <code>
        /// // Per-controller usage (MVC):
        /// // [EnableCors("cors-trusted")]
        /// // public class MyController : ControllerBase { ... }
        /// </code>
        /// </example>
        public static IApplicationBuilder UseWebQuarkCors(
            this IApplicationBuilder app,
            string defaultPolicy = null)
        {
            if (!string.IsNullOrWhiteSpace(defaultPolicy))
                app.UseCors(defaultPolicy);
            return app;
        }

        /// <summary>
        /// Configures ASP.NET Core CorsOptions based on WebQuarkCorsOptions.
        /// </summary>
        private sealed class ConfigureCorsOptions : IConfigureOptions<CorsOptions>
        {
            private readonly WebQuarkCorsOptions _o;

            public ConfigureCorsOptions(IOptions<WebQuarkCorsOptions> o) => _o = o.Value;

            public void Configure(CorsOptions options)
            {
                // --- Public: any origin, read-only methods, no credentials
                options.AddPolicy(_o.PublicPolicyName, b =>
                {
                    b.AllowAnyOrigin()
                     .AllowAnyHeader()
                     .WithMethods("GET", "HEAD", "OPTIONS")
                     .WithExposedHeaders(_o.ExposedHeaders)
                     .SetPreflightMaxAge(_o.PreflightMaxAge);
                });

                // --- Trusted: allowlist-driven origins with optional credentials
                options.AddPolicy(_o.TrustedPolicyName, b =>
                {
                    // Guardrail: credentials require an explicit allowlist (no "*").
                    if (_o.AllowCredentials && (_o.AllowedOrigins.Length == 0 || _o.AllowedOrigins.Contains("*")))
                        throw new InvalidOperationException("CORS: AllowCredentials requires an explicit allowlist (no '*').");

                    var originPredicate = OriginMatcher.Build(_o.AllowedOrigins);
                    b.SetIsOriginAllowed(originPredicate);

                    if (_o.AllowedMethods != null && _o.AllowedMethods.Length > 0) b.WithMethods(_o.AllowedMethods); else b.AllowAnyMethod();
                    if (_o.AllowedHeaders != null && _o.AllowedHeaders.Length > 0) b.WithHeaders(_o.AllowedHeaders); else b.AllowAnyHeader();

                    b.WithExposedHeaders(_o.ExposedHeaders)
                     .SetPreflightMaxAge(_o.PreflightMaxAge);

                    if (_o.AllowCredentials) b.AllowCredentials();
                });
            }
        }
    }
}
#elif LEGACY_OWIN
using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System.Web.Cors;
using WebQuark.Cors.Internal;
using WebQuark.Cors.Options;

namespace WebQuark.Cors.Extensions
{
    /// <summary>
    /// Extension methods to apply WebQuark CORS policies in OWIN (.NET Framework).
    /// </summary>
    public static class WebQuarkCorsExtensions
    {
        /// <summary>
        /// Applies the public (read-only, no credentials) policy across the OWIN pipeline.
        /// </summary>
        /// <param name="app">The OWIN app builder.</param>
        /// <param name="options">Optional CORS options; if null, defaults are used.</param>
        /// <returns>The same <see cref="IAppBuilder"/> for chaining.</returns>
        /// <example>
        /// <code>
        /// // Startup.cs (OWIN)
        /// public void Configuration(IAppBuilder app)
        /// {
        ///     app.UseWebQuarkCorsPublic(new WebQuark.Cors.Options.WebQuarkCorsOptions
        ///     {
        ///         PreflightMaxAge = TimeSpan.FromMinutes(30)
        ///     });
        /// }
        /// </code>
        /// </example>
        public static IAppBuilder UseWebQuarkCorsPublic(this IAppBuilder app, WebQuarkCorsOptions options = null)
        {
            var o = options ?? new WebQuarkCorsOptions();

            var policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                SupportsCredentials = false,
                AllowAnyHeader = true
            };
            policy.Methods.Add("GET");
            policy.Methods.Add("HEAD");
            policy.Methods.Add("OPTIONS");
            foreach (var h in o.ExposedHeaders) policy.ExposedHeaders.Add(h);

            var corsOptions = new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = _ => Task.FromResult(policy)
                }
            };

            // Preflight detection + Vary
            app.Use(async (ctx, next) =>
            {
                var isOptions = string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase);
                var origin    = ctx.Request.Headers.Get("Origin");
                var acrm      = ctx.Request.Headers.Get("Access-Control-Request-Method");

                if (isOptions && !string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(acrm))
                {
                    ctx.Response.Headers.Set("Access-Control-Max-Age", ((int)o.PreflightMaxAge.TotalSeconds).ToString());
                    ctx.Response.Headers.Set("Vary", "Origin, Access-Control-Request-Headers, Access-Control-Request-Method");
                }

                await next.Invoke();
            });

            return app.UseCors(corsOptions);
        }

        /// <summary>
        /// Applies the trusted allowlist policy (supports wildcard subdomains and optional credentials).
        /// </summary>
        /// <param name="app">The OWIN app builder.</param>
        /// <param name="options">CORS options (must not be null for 'trusted').</param>
        /// <returns>The same <see cref="IAppBuilder"/> for chaining.</returns>
        /// <example>
        /// <code>
        /// // Startup.cs (OWIN)
        /// public void Configuration(IAppBuilder app)
        /// {
        ///     var cors = new WebQuark.Cors.Options.WebQuarkCorsOptions
        ///     {
        ///         AllowedOrigins   = new[] { "https://portal.example.it", "http://localhost:3000" },
        ///         AllowCredentials = true, // requires explicit allowlist (no "*")
        ///         PreflightMaxAge  = TimeSpan.FromMinutes(30)
        ///     };
        ///
        ///     app.UseWebQuarkCorsTrusted(cors);
        /// }
        /// </code>
        /// </example>
        public static IAppBuilder UseWebQuarkCorsTrusted(this IAppBuilder app, WebQuarkCorsOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.AllowCredentials && (options.AllowedOrigins.Length == 0 || Array.IndexOf(options.AllowedOrigins, "*") >= 0))
                throw new InvalidOperationException("CORS: AllowCredentials richiede allowlist esplicita (niente '*').");

            var isOriginAllowed = OriginMatcher.Build(options.AllowedOrigins);

            var basePolicy = new CorsPolicy
            {
                AllowAnyHeader = options.AllowedHeaders == null,
                AllowAnyMethod = options.AllowedMethods == null,
                SupportsCredentials = options.AllowCredentials
            };

            if (options.AllowedMethods != null && options.AllowedMethods.Length > 0)
                foreach (var m in options.AllowedMethods) basePolicy.Methods.Add(m);

            if (options.AllowedHeaders != null && options.AllowedHeaders.Length > 0)
                foreach (var h in options.AllowedHeaders) basePolicy.Headers.Add(h);

            foreach (var h in options.ExposedHeaders) basePolicy.ExposedHeaders.Add(h);

            var provider = new CorsPolicyProvider
            {
                // Func<IOwinRequest, Task<CorsPolicy>>
                PolicyResolver = request =>
                {
                    // QUI: niente request.Request, perché IOwinRequest è già la request
                    var origin = request?.Headers?.Get("Origin");
                    if (!string.IsNullOrWhiteSpace(origin) && isOriginAllowed(origin))
                    {
                        var p = ClonePolicy(basePolicy);
                        p.Origins.Add(origin);
                        return Task.FromResult(p);
                    }

                    // Nessuna policy se origin non ammesso
                    return Task.FromResult<CorsPolicy>(null);
                }
            };

            // Preflight detection + Vary
            app.Use(async (ctx, next) =>
            {
                var isOptions = string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase);
                var origin    = ctx.Request.Headers.Get("Origin");
                var acrm      = ctx.Request.Headers.Get("Access-Control-Request-Method");

                if (isOptions && !string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(acrm))
                {
                    ctx.Response.Headers.Set("Access-Control-Max-Age", ((int)options.PreflightMaxAge.TotalSeconds).ToString());
                    ctx.Response.Headers.Set("Vary", "Origin, Access-Control-Request-Headers, Access-Control-Request-Method");
                }

                await next.Invoke();
            });

            return app.UseCors(new CorsOptions { PolicyProvider = provider });
        }
    
        /// <summary>
        /// Compatibility helper: maps "cors-public" / "cors-trusted" to the corresponding OWIN setup.
        /// </summary>
        /// <param name="app">The OWIN app builder.</param>
        /// <param name="defaultPolicy">Policy name to apply ("cors-public" or "cors-trusted").</param>
        /// <param name="options">Optional CORS options (required when using "cors-trusted").</param>
        /// <returns>The same <see cref="IAppBuilder"/> for chaining.</returns>
        /// <example>
        /// <code>
        /// // Startup.cs (OWIN)
        /// public void Configuration(IAppBuilder app)
        /// {
        ///     var opts = new WebQuark.Cors.Options.WebQuarkCorsOptions
        ///     {
        ///         AllowedOrigins   = new[] { "https://portal.example.it" },
        ///         AllowCredentials = true
        ///     };
        ///
        ///     // Apply by name using the compatibility helper:
        ///     app.UseWebQuarkCors("cors-trusted", opts);
        ///     // or:
        ///     // app.UseWebQuarkCors("cors-public");
        /// }
        /// </code>
        /// </example>
        public static IAppBuilder UseWebQuarkCors(this IAppBuilder app, string defaultPolicy, WebQuarkCorsOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(defaultPolicy)) return app;

            // Default names match WebQuarkCorsOptions defaults
            var namePublic = (options != null ? options.PublicPolicyName : new WebQuarkCorsOptions().PublicPolicyName);
            var nameTrusted = (options != null ? options.TrustedPolicyName : new WebQuarkCorsOptions().TrustedPolicyName);

            if (string.Equals(defaultPolicy, namePublic, StringComparison.OrdinalIgnoreCase))
                return app.UseWebQuarkCorsPublic(options);

            if (string.Equals(defaultPolicy, nameTrusted, StringComparison.OrdinalIgnoreCase))
            {
                if (options == null)
                    throw new InvalidOperationException("UseWebQuarkCors: per la policy 'trusted' è necessario passare un'istanza di WebQuarkCorsOptions.");
                return app.UseWebQuarkCorsTrusted(options);
            }

            // Nome sconosciuto: non applico nulla
            return app;
        }

        private static CorsPolicy ClonePolicy(CorsPolicy src)
        {
            var p = new CorsPolicy
            {
                AllowAnyHeader = src.AllowAnyHeader,
                AllowAnyMethod = src.AllowAnyMethod,
                AllowAnyOrigin = src.AllowAnyOrigin,
                SupportsCredentials = src.SupportsCredentials
            };
            foreach (var m in src.Methods) p.Methods.Add(m);
            foreach (var h in src.Headers) p.Headers.Add(h);
            foreach (var e in src.ExposedHeaders) p.ExposedHeaders.Add(e);
            foreach (var o in src.Origins) p.Origins.Add(o);
            return p;
        }
    }
}
#endif