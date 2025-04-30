// Importation des namespaces nécessaires pour le bon fonctionnement du contrôleur
using Microsoft.AspNetCore.Mvc; // Pour la gestion des actions HTTP et des vues
using Npgsql; // Pour la connexion à la base de données PostgreSQL (non utilisé ici mais inclus pour Dapper)
using SaveursInedites.Models; // Importation des modèles (par exemple pour les ingrédients ou autres entités liées)
using Dapper; // Pour la gestion des données avec Dapper (non utilisé ici mais inclus pour la gestion des requêtes SQL)

namespace SaveursInedites.Controllers
{
    // Contrôleur des ingrédients, généralement utilisé pour gérer les actions relatives aux ingrédients dans le site
    public class IngredientsController : Controller
    {
        // Action retournant la vue Index pour afficher la page des ingrédients
        public IActionResult Index()
        {
            return View(); // Affiche la vue "Index" (par défaut située dans Views/Ingredients/Index.cshtml)
        }
    }
}

