// Autorise uniquement les utilisateurs avec le rôle "admin" à accéder à ce contrôleur
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaveursInedites.Controllers;

// Attribut global : toutes les actions de ce contrôleur nécessitent le rôle "admin"
[Authorize(Roles = "admin")]
public class AdministrationController : Controller
{
    // Chaîne de connexion à la base de données
    private readonly string _connexionString;

    // Constructeur avec injection de la configuration (appsettings.json)
    public AdministrationController(IConfiguration configuration)
    {
        // Récupération de la chaîne de connexion nommée "Saveurs_Inedites"
        _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;

        // Si non trouvée, lève une exception pour signaler une erreur de configuration
        if (_connexionString == null)
        {
            throw new Exception("Error : Connexion string not found ! ");
        }
    }

    // Action par défaut du contrôleur, affiche la vue d'administration
    public IActionResult Index()
    {
        return View(); // retourne la vue Index.cshtml (dans Views/Administration)
    }
}
