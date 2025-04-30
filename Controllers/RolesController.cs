// Importation des namespaces nécessaires pour le bon fonctionnement du contrôleur
using Microsoft.AspNetCore.Mvc; // Pour les actions HTTP et les vues
using Npgsql; // Pour la gestion des connexions à la base de données PostgreSQL
using Dapper; // Pour l'exécution des requêtes SQL avec Dapper
using SaveursInedites.Models; // Pour l'importation des modèles de données (comme le modèle 'Role')
using System.Data; // Pour les types de données génériques (non utilisé ici mais souvent nécessaire dans les connexions DB)

namespace SaveursInedites.Controllers
{
    // Définition du contrôleur RolesController, dédié à la gestion des rôles dans l'application
    public class RolesController : Controller
    {
        // Attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de RolesController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public RolesController(IConfiguration configuration)
        {
            // Récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;
            // Si la chaîne de connexion n'est pas trouvée, on lève une exception
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // Action qui affiche tous les rôles dans la base de données
        public IActionResult Index()
        {
            // Requête SQL pour récupérer tous les rôles
            string query = "Select * from Roles";
            List<Role> roles;
            // Utilisation de Dapper pour exécuter la requête et récupérer les rôles
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                roles = connexion.Query<Role>(query).ToList(); // Récupère les rôles dans une liste
            }
            // Retourne la vue "Index" avec la liste des rôles
            return View(roles);
        }

        // Action pour afficher les détails d'un rôle spécifique
        public IActionResult Detail(int id)
        {
            // Requête SQL pour récupérer un rôle par son ID
            string query = "select * from roles where id=@id";
            Role role;
            // Utilisation de Dapper pour récupérer le rôle correspondant à l'ID
            using (var connexion = new NpgsqlConnection(_connexionString))
                try
                {
                    // Exécution de la requête et récupération du rôle
                    role = connexion.QuerySingle<Role>(query, new { id = id });
                }
                catch (System.Exception)
                {
                    // Si une erreur se produit (par exemple, aucun rôle trouvé), retourne une erreur 404
                    return NotFound();
                }
            // Retourne la vue "Detail" avec le rôle récupéré
            return View(role);
        }

        // Action pour afficher le formulaire de création d'un nouveau rôle
        [HttpGet]
        public IActionResult Nouveau()
        {
            // Retourne la vue "EditeurRole" pour l'ajout d'un nouveau rôle
            return View("EditeurRole");
        }

        // Action pour traiter le formulaire de création d'un rôle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Nouveau([FromForm] Role role)
        {
            // Vérifie que les données envoyées sont valides
            if (!ModelState.IsValid)
            {
                // Si non, retourne la vue avec un message d'erreur
                ViewData["ValidateMessage"] = "Erreur";
                return View("EditeurRole", role);
            }
            // Requête SQL pour insérer un nouveau rôle dans la base de données
            string query = "INSERT INTO Roles (nom) VALUES" +
                "(@nom)";
            int res;
            // Exécution de la requête d'insertion avec Dapper
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                res = connexion.Execute(query, role);
            }
            // Si l'insertion est réussie (1 ligne affectée), redirige vers la liste des rôles
            if (res == 1)
            {
                TempData["ValidateMessage"] = "Le rôle a bien été créé !";
                return RedirectToAction("Index");
            }
            else
            {
                // Si l'insertion échoue, retourne la vue avec un message d'erreur
                ViewData["ValidateMessage"] = "Erreur";
                return View("EditeurUtilisateur", role);
            }
        }
    }
}
