using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SaveursInedites.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SaveursInedites.Controllers
{
    public class UtilisateursController : Controller
    {
        // attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de RecettesController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public UtilisateursController(IConfiguration configuration)
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
            string query = "SELECT * FROM Utilisateurs";
            List<Utilisateur> utilisateurs;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                utilisateurs = connexion.Query<Utilisateur>(query).ToList();
            }
            return View(utilisateurs);
        }

        [HttpGet]
        public IActionResult Nouveau()
        {
            return View("NouveauUtilisateur");
        }
        [HttpPost]
        public IActionResult Nouveau([FromForm] Utilisateur utilisateur)
        {
            // validation côté serveur
            if (!ModelState.IsValid)
            {
                // si modèle pas valide -> renvoi le formulaire avec message d'erreur
                ViewData["ValidateMessage"] = "Erreur, veuillez réessayer.";
                return View("EditeurUtilisateur", utilisateur);
            }
            // si le modèle est valide
            string query = "INSERT INTO Utilisateurs(nom,prenom,email,date_inscription,role_id) VALUES (@nom,@prenom,@email,NOW(),@role_id)";
            int res;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    res = connexion.Execute(query, utilisateur);
                }
                catch (NpgsqlException e)
                {
                    if (e.SqlState == "23505")
                    {
                        ViewData["ValidateMessage"] = "Cette adresse email est déjà utilisée";
                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Erreur : veuillez réessayer plus tard.";
                    }
                    return View("EditeurUtilisateur", utilisateur);
                }
            }
            if (res == 1) // si la requête s'est bien passée
            {
                TempData["ValidateMessage"] = "L'utilisateur a bien été créé.";
                return RedirectToAction("Index");
            }
            else // si la requête ne s'est pas bien passée
            {
                ViewData["ValidateMessage"] = "Erreur, veuillez réessayer.";
                return View("EditeurUtilisateur", utilisateur);
            }
                    }

    }
}

