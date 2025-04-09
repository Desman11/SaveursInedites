using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using SaveursInedites.Models;
using System.Data;

namespace SaveursInedites.Controllers
{
    public class RolesController : Controller
    {
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de RolesController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public RolesController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }
        public IActionResult Index()
        {
            string query = "Select * from Roles";
            List<Role> roles;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                roles = connexion.Query<Role>(query).ToList();
            }
            return View(roles);
        }
        public IActionResult Detail(int id)
        {
            string query = "select * from roles where id=@id";
            Role role;
            using (var connexion = new NpgsqlConnection(_connexionString))
                try
                {
                    role = connexion.QuerySingle<Role>(query, new { id = id });
                }
                catch (System.Exception)
                {

                    return NotFound();
                }
            return View(role);


        }
        [HttpGet]
        public IActionResult Nouveau()
        {
            return View("EditeurRole");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Nouveau([FromForm] Role role)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Erreur";
                return View("EditeurRole", role);
            }
            string query = "INSERT INTO Roles (nom) VALUES" +
                "(@nom)";
            int res;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                res = connexion.Execute(query, role);
            }
            if (res == 1)
            {
                TempData["ValidateMessage"] = "Le role à bien créé !";
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["ValidateMessage"] = "Erreur";
                return View("EditeurUtilisateur", role);
            }
        }
    }

}



