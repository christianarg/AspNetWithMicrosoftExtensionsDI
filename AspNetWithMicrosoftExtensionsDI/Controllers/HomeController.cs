using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AspNetWithMicrosoftExtensionsDI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMyInterface myInterface;

        public HomeController(IHttpClientFactory httpClientFactory, IMyInterface myInterface)
        {
            this.httpClientFactory = httpClientFactory;
            this.myInterface = myInterface;
        }

        public async Task<ActionResult> Index()
        {
            var response = await httpClientFactory.CreateClient().GetAsync("https://www.google.es");
            var content = await response.Content.ReadAsStringAsync();
            ViewBag.Message = myInterface.Foo();
            ViewBag.Content = content;
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