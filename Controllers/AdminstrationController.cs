using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaveursInedites.Controllers;


[Authorize(Roles = "admin")]
public class AdministrationController : Controller
{

    private readonly string _connexionString;

    public AdministrationController(IConfiguration configuration)
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
        return View();
    }
}

