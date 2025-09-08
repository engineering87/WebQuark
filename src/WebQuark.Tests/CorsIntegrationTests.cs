// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under the MIT license (see LICENSE.txt for details)
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebQuark.Cors.Extensions;

namespace WebQuark.Tests
{
    public sealed class CorsIntegrationTests
    {
        [Fact]
        public void PublicPolicy_AllowsAnyOrigin_And_NoCredentials()
        {
            var services = new ServiceCollection()
                .AddWebQuarkCors(o =>
                {
                    // Prevent the trusted-policy guard from throwing when not used in this test
                    o.AllowCredentials = false;
                    o.ExposedHeaders = new[] { "X-Correlation-Id" };
                });

            using var sp = services.BuildServiceProvider();
            var opts = BuildCorsOptions(sp);
            var policy = opts.GetPolicy("cors-public");
            Assert.NotNull(policy);

            // ✔️ Any origin is allowed via the boolean flag
            Assert.True(policy!.AllowAnyOrigin);
            // ❌ Don't call policy.IsOriginAllowed(...) for public policy

            // No credentials in the public policy
            Assert.False(policy.SupportsCredentials);

            // Methods set as expected
            Assert.Contains("GET", policy.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("HEAD", policy.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("OPTIONS", policy.Methods, StringComparer.OrdinalIgnoreCase);

            // Exposed headers propagated
            Assert.Contains("X-Correlation-Id", policy.ExposedHeaders, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void TrustedPolicy_Allows_ExactOrigin_With_Credentials()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddWebQuarkCors(o =>
                {
                    o.AllowedOrigins = new[] { "app.mydomain.it" };
                    o.AllowCredentials = true;
                    o.AllowedMethods = new[] { "GET", "POST" };
                    o.ExposedHeaders = new[] { "X-Correlation-Id" };
                });

            var sp = services.BuildServiceProvider();

            var opts = BuildCorsOptions(sp);
            var policy = opts.GetPolicy("cors-trusted");
            Assert.NotNull(policy);

            // Act & Assert
            Assert.True(policy!.IsOriginAllowed?.Invoke("https://app.mydomain.it"));
            Assert.False(policy.IsOriginAllowed?.Invoke("https://evil.example.com"));

            Assert.True(policy.SupportsCredentials);

            // Methods
            Assert.Contains("GET", policy.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("POST", policy.Methods, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void TrustedPolicy_Wildcard_Subdomain_Matches_Deep_Subdomains_Not_Root()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddWebQuarkCors(o =>
                {
                    o.AllowedOrigins = new[] { "*.partner.gov.it" };
                    o.AllowCredentials = true; // credentials allowed (explicit allowlist, not "*")
                });

            var sp = services.BuildServiceProvider();

            var opts = BuildCorsOptions(sp);
            var policy = opts.GetPolicy("cors-trusted");
            Assert.NotNull(policy);

            // Act & Assert
            Assert.True(policy!.IsOriginAllowed?.Invoke("https://a.b.partner.gov.it")); // deep subdomain OK
            Assert.False(policy.IsOriginAllowed?.Invoke("https://partner.gov.it"));      // root NOT OK
        }

        [Fact]
        public void TrustedPolicy_Rejects_NonHttp_Schemes()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddWebQuarkCors(o =>
                {
                    o.AllowedOrigins = new[] { "*.example.com" };
                    o.AllowCredentials = false;
                });

            var sp = services.BuildServiceProvider();

            var opts = BuildCorsOptions(sp);
            var policy = opts.GetPolicy("cors-trusted");
            Assert.NotNull(policy);

            // Act & Assert
            Assert.False(policy!.IsOriginAllowed?.Invoke("ftp://files.example.com"));
            Assert.True(policy.IsOriginAllowed?.Invoke("https://app.example.com"));
        }

        [Fact]
        public void Guardrail_Throws_When_Credentials_With_AnyOrigin()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddWebQuarkCors(o =>
                {
                    o.AllowedOrigins = new[] { "*" };  // any origin
                    o.AllowCredentials = true;         // not allowed with "*"
                });

            // Act & Assert: applying Configure(CorsOptions) must throw
            var sp = services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => BuildCorsOptions(sp));
        }

        // ------------ helpers ------------

        private static CorsOptions BuildCorsOptions(ServiceProvider sp)
        {
            var configs = sp.GetServices<IConfigureOptions<CorsOptions>>().ToArray();
            var opts = new CorsOptions();
            foreach (var cfg in configs)
                cfg.Configure(opts);
            return opts;
        }
    }
}