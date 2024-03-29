﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Unity;

namespace AspNetWithMicrosoftExtensionsDI.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IHttpClientFactory HttpClientFactory { get; set; }
        [Dependency]
        public IMyInterface MyInterface { get; set; }
        [Dependency("paco")]
        public IMyInterface MyInterfaceNamed { get; set; }
        //private readonly IHttpClientFactory httpClientFactory;
        //private readonly IMyInterface myInterface;

        //public HomeController(IHttpClientFactory httpClientFactory, IMyInterface myInterface)
        //{
        //    this.httpClientFactory = httpClientFactory;
        //    this.myInterface = myInterface;
        //}

        public async Task<ActionResult> Index()
        {
            var response = await HttpClientFactory.CreateClient().GetAsync("https://www.google.es");
            var content = await response.Content.ReadAsStringAsync();
            ViewBag.Message = MyInterface.Foo();
            ViewBag.Message2 = MyInterfaceNamed.Foo();
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