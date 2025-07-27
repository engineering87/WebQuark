using Microsoft.AspNetCore.Mvc.RazorPages;
using WebQuark.Core.Interfaces;

namespace WebQuark.Tests.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpRequestInspector _requestInspector;
        private readonly IRequestQueryHandler _queryHandler;
        private readonly ISessionHandler _sessionHandler;

        public IndexModel(
            ILogger<IndexModel> logger,
            IHttpRequestInspector requestInspector,
            IRequestQueryHandler queryHandler,
            ISessionHandler sessionHandler)
        {
            _logger = logger;
            _requestInspector = requestInspector;
            _queryHandler = queryHandler;
            _sessionHandler = sessionHandler;
        }

        public void OnGet()
        {
            // Example usage
            string method = _requestInspector.GetHttpMethod();
            string userAgent = _requestInspector.GetUserAgent();

            var parameters = _queryHandler.ToDictionary();
            bool hasFoo = _queryHandler.HasKey("foo");
            string fooValue = _queryHandler.Get("foo", "default");

            _sessionHandler.SetString("LastVisit", DateTime.UtcNow.ToString("o"));
        }
    }
}