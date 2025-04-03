using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using BC = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using SaveursInedites.Models;
using System;


namespace SaveursInedites.Controllers;


public class AccessController : Controller
{

    private readonly string _connexionString;

    public AccessController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        // récupération de la chaîne de connexion dans la configuration
        _connexionString = configuration.GetConnectionString(("Saveurs_Inedites"))!;
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
        string query = "SELECT * FROM Utilisateurs WHERE email = @email";
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
                string insertQuery = "INSERT INTO Utilisateurs (identifiant,email,password,role_id) VALUES (@identifiant,@email,@password,2)";
                string HashedPassword = BC.HashPassword(utilisateur.password);

                int RowsAffected = connexion.Execute(insertQuery, new { identifiant = utilisateur.identifiant, email = utilisateur.email, password = HashedPassword });
                if (RowsAffected == 1)
                {
                    TempData["ValidateMessage"] = "Votre inscription est terminée. Veuillez vous connecter avec vos identifiants..";
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
        ModelState["identifiant"]!.ValidationState = ModelValidationState.Valid;
        

        if (!ModelState.IsValid)
        {
            ViewData["ValidateMessage"] = "Email ou mot de passe invalide.";
            return View();
        }
        string query = "SELECT utilisateurs.id,email, utilisateurs.identifiant, password, roles.id, roles.nom FROM Utilisateurs JOIN Roles ON utilisateurs.role_id = roles.id WHERE email = @email";
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            Utilisateur utilisateurDB;
            try
            {
                utilisateurDB = connexion.Query<Utilisateur, Role, Utilisateur>(query,
                (utilisateur, role) => {
                    utilisateur.role = role;
                    return utilisateur;
                },
                new { Email = utilisateur.email }
                , splitOn: "id").First();
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
                        new Claim(ClaimTypes.Email, utilisateur.email),
                        new Claim(ClaimTypes.NameIdentifier, utilisateurDB.Id.ToString()),
                        new Claim(ClaimTypes.Name, utilisateurDB.identifiant),
                        new Claim(ClaimTypes.Role,utilisateurDB.role.nom)
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
                ViewData["ValidateMessage"] = "Wrong email or password.";
                return View();
            }

        }
    }


    //[Authorize]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("SignIn", "Access");
    }


}

