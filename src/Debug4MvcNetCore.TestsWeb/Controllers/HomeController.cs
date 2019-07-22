using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Debug4MvcNetCore.TestsWeb.Models;
using Microsoft.Extensions.Logging;

namespace Debug4MvcNetCore.TestsWeb.Controllers
{
    public class HomeController : Controller
    {
        public ILogger<HomeController> Logger { get; }

        public HomeController(ILogger<HomeController> logger)
        {
            Logger = logger;
        }

        public IActionResult Index()
        {
            Logger.LogWarning("Index: test warning");
            return View();
        }

        public IActionResult Privacy()
        {
            Logger.LogWarning("Privacy: test warning");
            throw new Exception("Test error");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
