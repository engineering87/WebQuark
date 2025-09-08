// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
namespace WebQuark.Cors.Options
{
    public sealed class WebQuarkCorsOptions
    {
        public string PublicPolicyName { get; set; } = "cors-public";
        public string TrustedPolicyName { get; set; } = "cors-trusted";

        /// <summary>Allowlist per la policy "trusted". Supporta host esatti e wildcard subdomain (es. "*.example.it").</summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

        /// <summary>Abilita credenziali (cookie/Authorization). Vietato con '*' in AllowedOrigins.</summary>
        public bool AllowCredentials { get; set; } = true;

        /// <summary>HTTP methods ammessi (trusted). Null ⇒ qualsiasi.</summary>
        public string[] AllowedMethods { get; set; }

        /// <summary>Headers ammessi (trusted). Null ⇒ qualsiasi.</summary>
        public string[] AllowedHeaders { get; set; }

        /// <summary>Headers esposti al client.</summary>
        public string[] ExposedHeaders { get; set; } = Array.Empty<string>();

        /// <summary>Max-Age per il preflight.</summary>
        public TimeSpan PreflightMaxAge { get; set; } = TimeSpan.FromHours(1);
    }
}