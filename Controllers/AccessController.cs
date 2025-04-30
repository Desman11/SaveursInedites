// Inclusion des espaces de noms nécessaires
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using BC = BCrypt.Net.BCrypt; // Alias pour BCrypt
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using SaveursInedites.Models;
using System;

namespace SaveursInedites.Controllers;

// Déclaration du contrôleur d'accès (gestion inscription / connexion)
public class AccessController : Controller
{
    // Chaîne de connexion à la base PostgreSQL
    private readonly string _connexionString;

    // Constructeur : injecte le logger et la configuration
    public AccessController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;

        // Si la chaîne de connexion n'est pas trouvée, lève une exception
        if (_connexionString == null)
        {
            throw new Exception("Error : Connexion string not found ! ");
        }
    }

    /// <summary>
    /// Retourne le formulaire d'inscription
    /// </summary>
    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }

    // Traitement de l'inscription (formulaire)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignUp([FromForm] Utilisateur utilisateur)
    {
        string query = "SELECT * FROM Utilisateurs WHERE email = @email";

        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            // Vérifie si l'email est déjà utilisé
            var utilisateursDB = connexion.Query<Utilisateur>(query, new { Email = utilisateur.email }).ToList();

            if (utilisateursDB.Count > 0)
            {
                ViewData["ValidateMessage"] = "email déjà utilisé";
                return View();
            }
            else
            {
                // Hash le mot de passe puis insère le nouvel utilisateur
                string insertQuery = "INSERT INTO Utilisateurs (identifiant,email,password,role_id) VALUES (@identifiant,@email,@password,2)";
                string HashedPassword = BC.HashPassword(utilisateur.password);

                int RowsAffected = connexion.Execute(insertQuery, new
                {
                    identifiant = utilisateur.identifiant,
                    email = utilisateur.email,
                    password = HashedPassword
                });

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

    // Affiche le formulaire de connexion
    [HttpGet]
    public IActionResult SignIn()
    {
        return View();
    }

    // Traitement de la connexion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn([FromForm] Utilisateur utilisateur)
    {
        // Bypass validation de ce champ (facilite le test sans rendre obligatoire l’identifiant)
        ModelState["identifiant"]!.ValidationState = ModelValidationState.Valid;

        if (!ModelState.IsValid)
        {
            ViewData["ValidateMessage"] = "Email ou mot de passe invalide.";
            return View();
        }

        // Requête SQL avec jointure pour obtenir l'utilisateur et son rôle
        string query = @"SELECT utilisateurs.id, email, utilisateurs.identifiant, password, roles.id, roles.nom
                         FROM Utilisateurs
                         JOIN Roles ON utilisateurs.role_id = roles.id
                         WHERE email = @email";

        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            Utilisateur utilisateurDB;

            try
            {
                // Récupère l'utilisateur en liant les objets `Utilisateur` et `Role`
                utilisateurDB = connexion.Query<Utilisateur, Role, Utilisateur>(
                    query,
                    (utilisateur, role) => {
                        utilisateur.role = role;
                        return utilisateur;
                    },
                    new { Email = utilisateur.email },
                    splitOn: "id"
                ).First();
            }
            catch (InvalidOperationException)
            {
                Response.StatusCode = 403;
                ViewData["ValidateMessage"] = "Email ou mot de passe incorrect.";
                return View();
            }

            // Vérifie si le mot de passe est correct
            if (BC.Verify(utilisateur.password, utilisateurDB.password))
            {
                // Crée la liste des claims pour l'utilisateur
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, utilisateur.email),
                    new Claim(ClaimTypes.NameIdentifier, utilisateurDB.Id.ToString()),
                    new Claim(ClaimTypes.Name, utilisateurDB.identifiant),
                    new Claim(ClaimTypes.Role, utilisateurDB.role.nom)
                };

                // Crée l'identité et l'authentifie avec cookies
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    // IsPersistent = utilisateur.keepLoggedIn, (option à implémenter si tu veux "se souvenir de moi")
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    properties
                );

                // Redirige vers la page d'origine si précisée
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

    // Déconnexion de l'utilisateur
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("SignIn", "Access");
    }
}
