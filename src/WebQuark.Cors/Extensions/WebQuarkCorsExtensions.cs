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

            // Access-Control-Max-Age (non disponibile nativamente: lo impostiamo noi per OPTIONS)
            app.Use(async (ctx, next) =>
            {
                if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Headers.Set("Access-Control-Max-Age", ((int)o.PreflightMaxAge.TotalSeconds).ToString());
                }
                await next.Invoke();
            });

            return app.UseCors(corsOptions);
        }

        /// <summary>
        /// Applies the trusted allowlist policy (supports wildcard subdomains and optional credentials).
        /// </summary>
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

            // Access-Control-Max-Age per preflight
            app.Use(async (ctx, next) =>
            {
                if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Headers.Set("Access-Control-Max-Age", ((int)options.PreflightMaxAge.TotalSeconds).ToString());
                }
                await next.Invoke();
            });

            return app.UseCors(new CorsOptions { PolicyProvider = provider });
        }

        /// <summary>
        /// Compatibility helper: maps "cors-public" / "cors-trusted" to the corresponding OWIN setup.
        /// </summary>
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