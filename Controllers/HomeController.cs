using Microsoft.AspNetCore.Mvc;
using SaveursInedites.Models;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using Dapper;



namespace SaveursInedites.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Page d'accueil
        public IActionResult Index()
        {
            return View();
        }


        // Page "À propos"
        public IActionResult About()
        {
            return View();
        }

        // Page "Contact"
        public IActionResult Contact()
        {
            return View();
        }

          
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Cookies()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Route("/Home/HandleError/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 403)
            {
                return View("AccessDenied");
            }
            else if (statusCode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View("AutresErreurs");
            }
        }

    }
}

    
