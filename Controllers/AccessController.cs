using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using SaveursInedites.Models;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using BC = BCrypt.Net.BCrypt;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace SaveursInedites.Controllers
{
    public class AccessController : Controller
    {
        private readonly string _connexionString;

        public AccessController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        /// <summary>
        /// Retourne le forulaire d'inscription
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp([FromForm] Utilisateur utilisateur)
        {
                      string query = "SELECT * FROM utilisateurs WHERE email = @email";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                List<Utilisateur> utilisateursDB = connexion.Query<Utilisateur>(query, new { Email = utilisateur.email }).ToList();

                if (utilisateursDB.Count > 0)
                {
                    ViewData["ValidateMessage"] = "email déjà utilisé";
                    return View();
                }
                else
                {
                    string insertQuery = "INSERT INTO Utilisateurs (identifiant,email,password) VALUES (@identifiant,@email,@password)";

                    string HashedPassword = BC.HashPassword(utilisateur.password);

                    int RowsAffected = connexion.Execute(insertQuery, new { Identifiant = utilisateur.identifiant, Email = utilisateur.email, password = HashedPassword });
                    if (RowsAffected == 1)
                    {
                        TempData["ValidateMessage"] = "Votre inscription est terminée. Veuillez vous connecter avec vos identifiants.";
                        return RedirectToAction("SignIn");

                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Erreur lors du processus de connexion, veuillez réessayer.";
                        return View();
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
        {
         
            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Email ou mot de passe invalide.";
                return View();
            }
            string query = "SELECT email, password FROM Utilisateurs WHERE email = @email";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Utilisateur utilisateurDB;
                try
                {
                    utilisateurDB = connexion.QuerySingle<Utilisateur>(query, new { Email = utilisateur.email });
                }
                catch (InvalidOperationException)
                {
                    Response.StatusCode = 403;
                    ViewData["ValidateMessage"] = "Email ou mot de passe incorrect.";
                    return View();
                }

                if (BC.Verify(utilisateur.password, utilisateurDB.password))
                {
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, utilisateur.email),

                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        // IsPersistent = utilisateur.keepLoggedIn,
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                    if (Request.Form.ContainsKey("ReturnURL"))
                    {
                        return Redirect(Request.Form["ReturnURL"]!);
                    }
                    return RedirectToAction("Index", "Recettes");
                }
                else
                {
                    Response.StatusCode = 403;
                    ViewData["ValidateMessage"] = "Email ou mot de passe incorrect";
                    return View();
                }

            }
        }


        
        public async Task<IActionResult> LogOut()
        {
           // vous aurez besoin de modifier le type de retour de votre méthode en Task<IActionResult> (programmation asynchrone étudiée plus tard dans la formation)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Access");
        }

    }
}
