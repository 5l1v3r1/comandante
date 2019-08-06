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
        public Debug4MvcNetCoreTestsWebContext DbContext { get; }

        public HomeController(ILogger<HomeController> logger, Debug4MvcNetCoreTestsWebContext dbContext)
        {
            Logger = logger;
            DbContext = dbContext;
        }

        public IActionResult Index()
        {
            string name = "John";
            bool isEnabled = true;
            DbContext.Users.Where(x => x.UserName == name && x.LockoutEnabled == isEnabled).ToList();
            DbContext.Users.Count();
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
