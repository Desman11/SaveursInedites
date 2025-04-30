// Importation des namespaces nécessaires pour le bon fonctionnement du contrôleur
using Microsoft.AspNetCore.Mvc; // Pour les actions HTTP et les vues
using Npgsql; // Pour la gestion des connexions à la base de données PostgreSQL
using SaveursInedites.Models; // Pour l'importation des modèles de données (comme le modèle 'Utilisateur' et 'Role')
using Dapper; // Pour l'exécution des requêtes SQL avec Dapper
using Microsoft.AspNetCore.Mvc.Rendering; // Pour manipuler les listes déroulantes (SelectListItem)
using Microsoft.AspNetCore.Authorization; // Pour gérer les autorisations et rôles des utilisateurs

namespace SaveursInedites.Controllers
{
    // Définition du contrôleur UtilisateursController, dédié à la gestion des utilisateurs
    public class UtilisateursController : Controller
    {
        // Attribut stockant la chaîne de connexion à la base de données
        private readonly string _connexionString;

        /// <summary>
        /// Constructeur de UtilisateursController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public UtilisateursController(IConfiguration configuration)
        {
            // Récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;
            // Si la chaîne de connexion n'est pas trouvée, on lève une exception
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        // Action pour afficher tous les utilisateurs dans la base de données
        public IActionResult Index()
        {
            // Requête SQL pour récupérer tous les utilisateurs
            string query = "SELECT * FROM Utilisateurs";
            List<Utilisateur> utilisateurs;
            // Utilisation de Dapper pour exécuter la requête et récupérer les utilisateurs
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                utilisateurs = connexion.Query<Utilisateur>(query).ToList(); // Récupère les utilisateurs dans une liste
            }
            // Retourne la vue "Index" avec la liste des utilisateurs
            return View(utilisateurs);
        }

        // Action pour afficher le formulaire de création d'un nouvel utilisateur (accessible uniquement aux admins)
        [HttpGet]
        [Authorize(Roles = "admin")] // La page est accessible uniquement aux utilisateurs ayant le rôle "admin"
        public IActionResult Nouveau()
        {
            // Requête SQL pour récupérer tous les rôles dans la base de données
            string queryRoles = "SELECT * FROM Roles";
            List<Role> roles;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                roles = connexion.Query<Role>(queryRoles).ToList(); // Récupère les rôles dans une liste
            }

            // Création de la liste des items pour la liste déroulante des rôles
            List<SelectListItem> listeRoles = new List<SelectListItem>();
            foreach (Role role in roles)
            {
                listeRoles.Add(new SelectListItem(role.nom, role.id.ToString())); // Ajoute chaque rôle à la liste
            }

            // Transmet la liste des rôles à la vue sous forme de ViewData
            ViewData["listeRoles"] = listeRoles;
            // Retourne la vue "NouveauUtilisateur" pour afficher le formulaire de création
            return View("NouveauUtilisateur");
        }

        // Action pour traiter le formulaire de création d'un nouvel utilisateur (accessible uniquement aux admins)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")] // La page est accessible uniquement aux utilisateurs ayant le rôle "admin"
        public IActionResult Nouveau([FromForm] Utilisateur utilisateur)
        {
            // Vérifie que les données du modèle sont valides
            if (!ModelState.IsValid)
            {
                // Si le modèle n'est pas valide, retourne le formulaire avec un message d'erreur
                ViewData["ValidateMessage"] = "Erreur, veuillez réessayer.";
                return View("EditeurUtilisateur", utilisateur);
            }

            // Si le modèle est valide, prépare la requête d'insertion dans la base de données
            string query = "INSERT INTO Utilisateurs(nom,prenom,email,role_id) VALUES (@nom,@prenom,@email,@role_id)";
            int res;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    // Exécute la requête d'insertion dans la base de données
                    res = connexion.Execute(query, utilisateur);
                }
                catch (NpgsqlException e)
                {
                    // Si une exception est levée (par exemple, l'email est déjà utilisé)
                    if (e.SqlState == "23505")
                    {
                        // Message d'erreur si l'email est déjà utilisé
                        ViewData["ValidateMessage"] = "Cette adresse email est déjà utilisée";
                    }
                    else
                    {
                        // Message d'erreur générique en cas de problème
                        ViewData["ValidateMessage"] = "Erreur : veuillez réessayer plus tard.";
                    }
                    // Retourne le formulaire avec un message d'erreur
                    return View("EditeurUtilisateur", utilisateur);
                }
            }

            // Si l'insertion est réussie (1 ligne affectée), redirige vers la liste des utilisateurs
            if (res == 1)
            {
                TempData["ValidateMessage"] = "L'utilisateur a bien été créé.";
                return RedirectToAction("Index");
            }
            else
            {
                // Si l'insertion échoue, retourne le formulaire avec un message d'erreur
                ViewData["ValidateMessage"] = "Erreur, veuillez réessayer.";
                return View("EditeurUtilisateur", utilisateur);
            }
        }
    }
}
