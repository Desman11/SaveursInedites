// Importation des namespaces n�cessaires pour le fonctionnement du contr�leur
using Microsoft.AspNetCore.Mvc; // Pour la gestion des actions HTTP et des vues
using SaveursInedites.Models; // Importation des mod�les utilis�s dans les vues (par exemple ErrorViewModel)
using System.Linq; // Pour les op�rations LINQ (non utilis� ici mais potentiellement utile)
using System.Diagnostics; // Pour la gestion du suivi des erreurs
using Npgsql; // Pour la connexion � la base de donn�es PostgreSQL (non utilis� ici mais inclus pour Dapper)
using Dapper; // Pour l'utilisation de Dapper (non utilis� ici mais inclus pour la gestion des donn�es)
using Microsoft.AspNetCore.Authorization; // Pour g�rer les autorisations sur les actions

namespace SaveursInedites.Controllers
{
    // Attribut autorisant l'acc�s uniquement aux utilisateurs connect�s
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger pour le suivi des erreurs ou actions dans ce contr�leur

        // Constructeur avec injection de d�pendances (pour le logger)
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger; // Initialisation du logger pour �tre utilis� dans les actions du contr�leur
        }

        // Action retournant la page d'accueil (vue Index)
        public IActionResult Index()
        {
            return View(); // Affiche la vue par d�faut, g�n�ralement situ�e dans Views/Home/Index.cshtml
        }

        // Action retournant la page "� propos"
        public IActionResult About()
        {
            return View(); // Affiche la vue "About" (par d�faut dans Views/Home/About.cshtml)
        }

        // Action retournant la page "Contact"
        public IActionResult Contact()
        {
            return View(); // Affiche la vue "Contact" (par d�faut dans Views/Home/Contact.cshtml)
        }

        // Action retournant la page "Privacy" (politique de confidentialit�)
        public IActionResult Privacy()
        {
            return View(); // Affiche la vue "Privacy" (par d�faut dans Views/Home/Privacy.cshtml)
        }

        // Action retournant la page "Cookies" (informations sur les cookies)
        public IActionResult Cookies()
        {
            return View(); // Affiche la vue "Cookies" (par d�faut dans Views/Home/Cookies.cshtml)
        }

        // Gestion des erreurs, cache d�sactiv� pour le contr�le des erreurs (ne conserve pas l'erreur dans le cache)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Cr�e une vue d'erreur en passant l'ID de la requ�te ou l'identifiant de trace
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Route personnalis�e pour g�rer les erreurs selon le code d'�tat HTTP (par exemple 403 ou 404)
        [Route("/Home/HandleError/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            // Redirige vers des vues personnalis�es selon le code d'erreur HTTP
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
                return View("AutresErreurs"); // Affiche une vue g�n�rique pour les autres erreurs
            }
        }
    }
}
