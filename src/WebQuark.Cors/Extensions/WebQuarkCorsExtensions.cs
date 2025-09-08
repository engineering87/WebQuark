// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under the MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebQuark.Cors.Internal;
using WebQuark.Cors.Options;

namespace WebQuark.Cors.Extensions;

/// <summary>
/// Extension methods to register and apply WebQuark CORS policies.
/// </summary>
public static class WebQuarkCorsExtensions
{
    /// <summary>
    /// Registers WebQuark CORS with two named policies: a public read-only policy and a trusted allowlist policy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Optional configuration callback for <see cref="WebQuarkCorsOptions"/>.
    /// If omitted, defaults are used (safe-by-default).
    /// </param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWebQuarkCors(
        this IServiceCollection services,
        Action<WebQuarkCorsOptions>? configure = null)
    {
        services.AddOptions<WebQuarkCorsOptions>();
        if (configure is not null) services.Configure(configure);

        services.AddCors();
        services.AddSingleton<IConfigureOptions<CorsOptions>, ConfigureCorsOptions>();
        return services;
    }

    /// <summary>
    /// Applies a default CORS policy to the pipeline. If you need per-endpoint control, omit this and use <c>RequireCors</c> on individual endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="defaultPolicy">
    /// The name of the default policy to apply (e.g., <c>"cors-public"</c> or <c>"cors-trusted"</c>).
    /// When <see langword="null"/> or whitespace, no default policy is applied.
    /// </param>
    /// <returns>The original <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseWebQuarkCors(
        this IApplicationBuilder app,
        string? defaultPolicy = null)
    {
        if (!string.IsNullOrWhiteSpace(defaultPolicy))
            app.UseCors(defaultPolicy);
        return app;
    }

    /// <summary>
    /// Configures ASP.NET Core <see cref="CorsOptions"/> based on <see cref="WebQuarkCorsOptions"/>.
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

                if (_o.AllowedMethods is { Length: > 0 }) b.WithMethods(_o.AllowedMethods); else b.AllowAnyMethod();
                if (_o.AllowedHeaders is { Length: > 0 }) b.WithHeaders(_o.AllowedHeaders); else b.AllowAnyHeader();

                b.WithExposedHeaders(_o.ExposedHeaders)
                 .SetPreflightMaxAge(_o.PreflightMaxAge);

                if (_o.AllowCredentials) b.AllowCredentials();
                // If AllowCredentials() is not called, the policy remains without credentials by default.
            });
        }
    }
}