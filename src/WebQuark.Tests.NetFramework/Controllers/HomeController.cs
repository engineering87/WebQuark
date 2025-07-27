using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebQuark.HttpRequest;
using WebQuark.HttpResponse;
using WebQuark.QueryString;
using WebQuark.Session;

namespace WebQuark.Tests.NetFramework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var responseHandler = new HttpResponseHandler();
            var queryHandler = new QueryStringHandler(); // test ok on 4.7.2
            var sessionHandler = new SessionHandler(); // test ok on 4.7.2
            var requestInspector = new HttpRequestInspector(); // test ok on 4.7.2

            string httpMethod = requestInspector.GetHttpMethod();
            string userAgent = requestInspector.GetUserAgent();
            string ip = requestInspector.GetClientIpAddress();

            string fooValue = queryHandler.Get("foo", "defaultValue");
            queryHandler.Set("bar", "baz");

            sessionHandler.SetString("LastVisit", DateTime.Now.ToString());
            string lastVisit = sessionHandler.GetString("LastVisit");

            responseHandler.SetHeader("X-Custom-Header", "MyValue");

            ViewBag.HttpMethod = httpMethod;
            ViewBag.UserAgent = userAgent;
            ViewBag.ClientIP = ip;
            ViewBag.FooQueryValue = fooValue;
            ViewBag.LastVisitSession = lastVisit;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}