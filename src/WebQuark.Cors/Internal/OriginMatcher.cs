// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under the MIT license (see LICENSE.txt for details)
using System.Text.RegularExpressions;

namespace WebQuark.Cors.Internal
{
    /// <summary>
    /// Utilities to build host-matching predicates for CORS Origin validation.
    /// </summary>
    internal static class OriginMatcher
    {
        /// <summary>
        /// Builds a predicate that evaluates whether a given Origin is allowed.
        /// </summary>
        /// <param name="patterns">
        /// List of allowed origins:
        /// <list type="bullet">
        /// <item><description>Exact host (e.g., <c>app.example.com</c>)</description></item>
        /// <item><description>Wildcard subdomain (e.g., <c>*.example.com</c>)</description></item>
        /// <item><description><c>"*"</c> to allow any origin (use ONLY when credentials are disabled)</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// A predicate that returns <see langword="true"/> if the <c>Origin</c> header matches the allowlist.
        /// </returns>
        internal static Func<string, bool> Build(string[] patterns)
        {
            // No patterns → nothing is allowed.
            if (patterns is null || patterns.Length == 0)
                return _ => false;

            // Wildcard-any: allow any origin (ONLY safe when credentials are NOT enabled).
            if (patterns.Any(p => p == "*"))
                return _ => true;

            // Precompile all host matchers.
            var matchers = patterns.Select(ToMatcher).ToArray();

            return origin =>
            {
                // Basic sanity checks.
                if (string.IsNullOrWhiteSpace(origin)) return false;
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;

                // Restrict to http/https only (disallow custom schemes).
                var scheme = uri.Scheme;
                if (!scheme.Equals("https", StringComparison.OrdinalIgnoreCase) &&
                    !scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Test host against all compiled matchers.
                var host = uri.Host;
                foreach (var m in matchers)
                    if (m(host)) return true;

                return false;
            };
        }

        /// <summary>
        /// Converts a single allowlist pattern into a host-matching function.
        /// </summary>
        /// <param name="pattern">
        /// Either an exact host (e.g., <c>app.example.com</c>) or a wildcard subdomain (e.g., <c>*.example.com</c>).
        /// </param>
        /// <returns>A predicate that matches a host string.</returns>
        private static Func<string, bool> ToMatcher(string pattern)
        {
            var p = pattern.Trim();

            // Exact host match: case-insensitive equality.
            if (!p.StartsWith("*.", StringComparison.Ordinal))
                return h => string.Equals(h, p, StringComparison.OrdinalIgnoreCase);

            // Wildcard subdomain match:
            //   "*.example.com" → must match "x.example.com", "a.b.example.com", etc., but NOT "example.com".
            var root = Regex.Escape(p.Substring(2));
            var rx = new Regex(
                @"^([a-z0-9-]+\.)+" + root + "$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
            );

            return h => rx.IsMatch(h);
        }
    }
}