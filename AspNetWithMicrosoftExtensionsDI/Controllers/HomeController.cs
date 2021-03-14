using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetWithMicrosoftExtensionsDI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMyInterface myInterface;

        public HomeController(IMyInterface myInterface)
        {
            this.myInterface = myInterface;
        }

        public ActionResult Index()
        {
            ViewBag.Message = myInterface.Foo();

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