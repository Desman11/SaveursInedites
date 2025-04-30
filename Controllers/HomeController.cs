// Importation des namespaces nécessaires pour le fonctionnement du contrôleur
using Microsoft.AspNetCore.Mvc; // Pour la gestion des actions HTTP et des vues
using SaveursInedites.Models; // Importation des modèles utilisés dans les vues (par exemple ErrorViewModel)
using System.Linq; // Pour les opérations LINQ (non utilisé ici mais potentiellement utile)
using System.Diagnostics; // Pour la gestion du suivi des erreurs
using Npgsql; // Pour la connexion à la base de données PostgreSQL (non utilisé ici mais inclus pour Dapper)
using Dapper; // Pour l'utilisation de Dapper (non utilisé ici mais inclus pour la gestion des données)
using Microsoft.AspNetCore.Authorization; // Pour gérer les autorisations sur les actions

namespace SaveursInedites.Controllers
{
    // Attribut autorisant l'accès uniquement aux utilisateurs connectés
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger pour le suivi des erreurs ou actions dans ce contrôleur

        // Constructeur avec injection de dépendances (pour le logger)
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger; // Initialisation du logger pour être utilisé dans les actions du contrôleur
        }

        // Action retournant la page d'accueil (vue Index)
        public IActionResult Index()
        {
            return View(); // Affiche la vue par défaut, généralement située dans Views/Home/Index.cshtml
        }

        // Action retournant la page "À propos"
        public IActionResult About()
        {
            return View(); // Affiche la vue "About" (par défaut dans Views/Home/About.cshtml)
        }

        // Action retournant la page "Contact"
        public IActionResult Contact()
        {
            return View(); // Affiche la vue "Contact" (par défaut dans Views/Home/Contact.cshtml)
        }

        // Action retournant la page "Privacy" (politique de confidentialité)
        public IActionResult Privacy()
        {
            return View(); // Affiche la vue "Privacy" (par défaut dans Views/Home/Privacy.cshtml)
        }

        // Action retournant la page "Cookies" (informations sur les cookies)
        public IActionResult Cookies()
        {
            return View(); // Affiche la vue "Cookies" (par défaut dans Views/Home/Cookies.cshtml)
        }

        // Gestion des erreurs, cache désactivé pour le contrôle des erreurs (ne conserve pas l'erreur dans le cache)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Crée une vue d'erreur en passant l'ID de la requête ou l'identifiant de trace
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Route personnalisée pour gérer les erreurs selon le code d'état HTTP (par exemple 403 ou 404)
        [Route("/Home/HandleError/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            // Redirige vers des vues personnalisées selon le code d'erreur HTTP
            if (statusCode == 403)
            {
                return View("AccessDenied"); // Affiche la vue "AccessDenied" si le code d'erreur est 403
            }
            else if (statusCode == 404)
            {
                return View("NotFound"); // Affiche la vue "NotFound" si le code d'erreur est 404
            }
            else
            {
                return View("AutresErreurs"); // Affiche une vue générique pour les autres erreurs
            }
        }
    }
}
