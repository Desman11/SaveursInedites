using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SaveursInedites.Models;
using Dapper;

namespace SaveursInedites.Controllers
{
    public class IngredientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
