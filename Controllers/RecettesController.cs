// Importation des namespaces nécessaires
using Microsoft.AspNetCore.Mvc;
using SaveursInedites.Models;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SaveursInedites.Controllers;
using System.Collections.Generic;

// Déclaration de l'espace de noms du contrôleur
namespace SaveursInedites.Controllers;

// Attribut pour exiger que l'utilisateur soit authentifié pour accéder à ce contrôleur
[Authorize]
public class RecettesController : Controller
{
    // Chaîne de connexion à la base de données PostgreSQL
    private readonly string _connexionString;

    // Constructeur du contrôleur, injection du logger et de la configuration
    public RecettesController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        // Récupération de la chaîne de connexion à partir du fichier de configuration
        _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;

        // Vérifie que la chaîne de connexion est bien récupérée, sinon déclenche une exception
        if (_connexionString == null)
        {
            throw new Exception("Error : Connexion string not found ! ");
        }
    }

    // Action principale : affiche la liste de toutes les recettes
    [Authorize]
    public IActionResult Index()
    {
        string query = "SELECT * FROM Recettes"; // Requête SQL
        List<Recette> recettes; // Liste de recettes à retourner à la vue

        // Connexion à la base et exécution de la requête
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            recettes = connexion.Query<Recette>(query).ToList(); // Récupération des données avec Dapper
        }

        return View(recettes); // Envoi de la liste à la vue
    }

    // Action qui affiche le détail complet d'une recette donnée par son id
    [Authorize]
    public IActionResult Detail(int id)
    {
        // Requêtes SQL pour récupérer les différents éléments liés à la recette
        string queryRecette = "SELECT * FROM recettes WHERE id = @id";
        string queryEtapes = "SELECT numero, texte FROM etapes WHERE id_recette = @id ORDER BY numero";
        string queryIngredients = "SELECT i.* FROM ingredients i JOIN ingredients_recettes ir ON i.id = ir.id_ingredient WHERE ir.id_recette = @id";
        string queryAvis = "SELECT note, commentaire FROM avis WHERE id_recette = @id";
        string queryMoyenne = "SELECT AVG(note) FROM avis WHERE id_recette = @id";

        Recette recette;

        // Connexion à la base et exécution des différentes requêtes
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            try
            {
                connexion.Open(); // Ouverture de la connexion

                // Récupération de la recette principale
                recette = connexion.QuerySingleOrDefault<Recette>(queryRecette, new { id });
                if (recette == null)
                    return NotFound(); // Si la recette n'existe pas, renvoyer une 404

                // Récupération des étapes de la recette
                recette.Etapes = connexion.Query<Etape>(queryEtapes, new { id }).ToList();

                // Récupération des ingrédients associés
                recette.Ingredients = connexion.Query<Ingredient>(queryIngredients, new { id }).ToList();

                // Récupération des avis utilisateurs
                var avisListe = connexion.Query<AvisViewModel>(queryAvis, new { id }).ToList();

                // Calcul de la moyenne des notes
                double? moyenne = connexion.ExecuteScalar<double?>(queryMoyenne, new { id });

                // Construction du ViewModel contenant toutes les données
                var model = new RecetteAvisViewModel
                {
                    Recette = recette,
                    NouvelAvis = new AvisViewModel { id_recette = id }, // Pour le formulaire d'avis
                    AvisExistants = avisListe,
                    MoyenneNote = moyenne ?? 0
                };

                // Passage de données supplémentaires à la vue
                ViewData["titrePage"] = recette.nom;
                ViewData["Moyenne"] = moyenne;

                return View(model); // Rendu de la vue avec le modèle
            }
            catch (Exception ex)
            {
                // En cas d'erreur : journalisation et retour code 500
                Console.WriteLine($"Erreur lors de la récupération des détails de la recette : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur");
            }
        }
    }

    // Attribut indiquant que cette action répond aux requêtes POST
    [HttpPost]
    // Protection contre les attaques CSRF (Cross-Site Request Forgery)
    [ValidateAntiForgeryToken]
    public IActionResult AjouterAvis([FromForm] RecetteAvisViewModel model)
    {
        // Récupère l'avis soumis depuis le ViewModel
        var avis = model.NouvelAvis;

        // Ignore la validation automatique des champs "Recette" et "AvisExistants" du modèle
        ModelState["Recette"].ValidationState = ModelValidationState.Skipped;
        ModelState["AvisExistants"].ValidationState = ModelValidationState.Skipped;

        // Vérifie si le modèle est invalide ou si la note est hors des bornes autorisées (1 à 5)
        if (!ModelState.IsValid || avis.Note < 1 || avis.Note > 5)
        {
            // Message d’erreur temporaire pour l'utilisateur
            TempData["Message"] = "Veuillez remplir tous les champs correctement.";
            // Redirection vers la page de détail de la recette
            return RedirectToAction("Detail", "Recettes", new { id = avis.id_recette });
        }

        // Connexion à la base de données pour enregistrer l'avis
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            // Requête SQL d'insertion de l'avis
            string query = "INSERT INTO Avis (id_recette,id_utilisateur, note, commentaire) VALUES (@id_recette,@id_utilisateur, @note, @commentaire)";

            // Exécution de la requête avec les paramètres de l'avis
            connexion.Execute(query, new
            {
                id_recette = avis.id_recette,
                id_utilisateur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), // ID de l'utilisateur connecté
                note = avis.Note,
                commentaire = avis.Commentaire
            });
        }

        // Message de confirmation temporaire à afficher après soumission
        TempData["Message"] = "Merci pour votre avis !";
        // Redirection vers la page de détail de la recette
        return RedirectToAction("Detail", "Recettes", new { id = avis.id_recette });
    }

    // Construit une liste déroulante des catégories à partir de la base
    private List<SelectListItem> ConstruireListeCategories()
    {
        // Requête pour récupérer toutes les catégories
        string queryCategories = "SELECT * FROM Categories";
        List<Categorie> categories;

        // Connexion à la base de données pour exécuter la requête
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            categories = connexion.Query<Categorie>(queryCategories).ToList();
        }

        // Transformation des catégories en éléments de liste sélectionnable pour les formulaires
        List<SelectListItem> liste_cat = new List<SelectListItem>();
        foreach (Categorie categorie in categories)
        {
            liste_cat.Add(new SelectListItem(categorie.Nom, categorie.Id.ToString()));
        }

        return liste_cat;
    }

    // Affiche le formulaire pour créer une nouvelle recette (GET)
    [HttpGet]
    public IActionResult Nouveau()
    {
        // Initialise le ViewModel pour l'éditeur de recette
        EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();

        // Remplit la liste des catégories pour le formulaire
        viewModel.categories = ConstruireListeCategories();

        // Donne l'action à afficher dans la vue (utile pour réutiliser la vue en mode édition ou création)
        ViewData["ActionFormulaire"] = "Nouveau";

        // Retourne la vue "EditeurRecette" avec le ViewModel préparé
        return View("EditeurRecette", viewModel);
    }
    // Action pour enregistrer une nouvelle recette (POST)
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nouveau([FromForm] Recette recette)
    {
        string? filePath = null; // chemin de la nouvelle photo
        string? anciennePhoto = null; // photo précédente si remplacement

        try
        {
            // Vérifie la validité du modèle
            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle n'est pas valide");
            }

            // Extensions de fichiers acceptées
            string[] permittedExtensions = { ".jpeg", ".jpg", ".png", ".gif" };
            var ext = Path.GetExtension(recette.photoFile.FileName).ToLowerInvariant();

            // Vérifie si le fichier est valide
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                ModelState["recette.photoFile"]!.Errors.Add(new ModelError("Ce type de fichier n'est pas accepté."));
                throw new Exception("Ce type de fichier n'est pas accepté.");
            }

            // Récupère l’ID de l’utilisateur connecté
            string? idUtilisateur = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUtilisateur))
            {
                return Unauthorized(); // Si pas connecté
            }

            // Attribue l’ID du créateur à la recette
            recette.Createur = int.Parse(idUtilisateur);

            // Requête pour insérer une recette
            string query = @"
        INSERT INTO Recettes (nom, temps_preparation, temps_cuisson, difficulte, photo, createur) 
        VALUES (@nom, @temps_preparation, @temps_cuisson, @difficulte, @photo, @createur)
        RETURNING id;";

            // Requête pour insérer la liaison catégorie-recette
            string queryRecetteCategorie = "INSERT INTO categories_recettes(id_categorie, id_recette) VALUES(@id_categorie, @id_recette)";

            // Enregistrement de la photo si elle existe
            if (recette.photoFile != null && recette.photoFile.Length > 0)
            {
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.photoFile.FileName));

                // Copie du fichier dans le dossier wwwroot/images/recettes
                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.photoFile.CopyTo(stream);
                }

                anciennePhoto = recette.photo;
                recette.photo = filePath;
            }

            // Insertion dans la base de données
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();

                // Démarre une transaction
                using (var transaction = connexion.BeginTransaction())
                {
                    // Exécute la requête d’insertion de la recette et récupère son ID
                    int recette_id = connexion.ExecuteScalar<int>(query, recette);

                    // Si échec de création
                    if (recette_id == 0)
                    {
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + recette.photo);
                        }

                        transaction.Rollback();
                        throw new Exception("Erreur pendant la création de la recette.");
                    }
                    else
                    {
                        // Prépare la liste des associations recette-catégorie
                        List<object> list = new List<object>();
                        foreach (int categorie_id in recette.categories_ids)
                        {
                            list.Add(new { id_recette = recette_id, id_categorie = categorie_id });
                        }

                        // Insère les liaisons dans la table pivot
                        int res = connexion.Execute(queryRecetteCategorie, list);

                        // Si une insertion a échoué
                        if (res != recette.categories_ids.Count)
                        {
                            if (anciennePhoto != null)
                            {
                                System.IO.File.Delete("wwwroot" + recette.photo);
                            }

                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }

                        // Tout s’est bien passé => commit
                        transaction.Commit();

                        // Supprime l’ancienne photo s’il y en avait une
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + anciennePhoto);
                        }
                    }
                }
            }

            // Redirige vers la liste des recettes
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // En cas d’erreur : message d'erreur dans le modèle et retour du formulaire
            ModelState.AddModelError("", ex.Message);
            return View(recette);
        }
    }

    // [Authorize(Roles = "admin")] ← à activer si on veut restreindre la modification aux administrateurs
    [HttpGet]
    public IActionResult Modifier(int id)
    {
        // Création du ViewModel utilisé pour afficher et modifier une recette
        EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();

        // Remplissage de la liste des catégories pour la liste déroulante dans le formulaire
        viewModel.categories = ConstruireListeCategories();

        // Requêtes SQL pour récupérer la recette à modifier et ses catégories associées
        string queryRecette = "SELECT * FROM Recettes WHERE recettes.id=@identifiant";
        string queryCategories = "SELECT id_categorie FROM categories_recettes WHERE id_recette=@identifiant";

        // Connexion à la base de données
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            try
            {
                // Récupération de la recette avec l'identifiant donné
                viewModel.recette = connexion.QueryFirst<Recette>(queryRecette, new { identifiant = id });

                // Récupération des ID des catégories associées à la recette
                viewModel.recette.categories_ids = connexion.Query<int>(queryCategories, new { identifiant = id }).ToList();
            }
            catch (Exception e)
            {
                // Si la recette n'est pas trouvée ou erreur SQL, retourne une erreur 404
                return NotFound();
            }
        }

        // Définit le nom de l’action à afficher dans la vue (utilisé pour les boutons ou titres)
        ViewData["ActionFormulaire"] = "Modifier";

        // Permet à la vue d'afficher ou non l'image de la recette actuelle
        if (viewModel.recette.photo != null)
        {
            ViewData["AfficherPhoto"] = true;
        }
        else
        {
            ViewData["AfficherPhoto"] = false;
        }

        // Retourne la vue "EditeurRecette" avec les données récupérées
        return View("EditeurRecette", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // [EstCreateurAuthorize] ← Middleware à activer si tu veux restreindre l'accès au créateur uniquement
    // [Authorize(Roles = "admin")] ← À activer si seuls les admins peuvent modifier
    public IActionResult Modifier([FromForm] Recette recette, [FromForm] string supprimerPhoto)
    {
        try
        {
            // Vérifie si le modèle est valide
            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle est pas valide");
            }

            // Variables pour la gestion de la photo
            string? filePath = null;           // chemin vers la nouvelle photo
            string? anciennePhoto = null;      // chemin vers l’ancienne photo

            // Si une nouvelle photo a été téléchargée
            if (recette.photoFile != null && recette.photoFile.Length > 0)
            {
                // Génère un nom de fichier aléatoire et extension correcte
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) +
                    Path.GetExtension(recette.photoFile.FileName)).ToString();

                // Sauvegarde la photo dans le dossier wwwroot/images/recettes
                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.photoFile.CopyTo(stream);
                }

                // Sauvegarde l’ancienne photo pour suppression après
                anciennePhoto = recette.photo;
                // Met à jour la propriété photo dans l’objet recette
                recette.photo = filePath;
            }

            // Requête SQL pour mettre à jour les données principales de la recette
            string query = "UPDATE recettes SET nom=@nom, temps_preparation=@temps_preparation, temps_cuisson=@temps_cuisson, difficulte=@difficulte, photo=@photo  WHERE id=@id";

            // Connexion et transaction
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var transaction = connexion.BeginTransaction())
                {
                    // Exécution de la mise à jour principale
                    int res = connexion.Execute(query, recette);
                    if (res != 1)
                    {
                        // En cas d’échec, supprime la photo nouvellement chargée
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + recette.photo);
                        }
                        transaction.Rollback();
                        throw new Exception("Erreur pendant la mise à jour de la recette");
                    }
                    else
                    {
                        // Mise à jour des catégories :
                        // Étape 1 - suppression des anciennes associations
                        string queryDelete = "DELETE FROM categories_recettes WHERE id_recette=@identifiant";
                        connexion.Execute(queryDelete, new { identifiant = recette.id });

                        // Étape 2 - insertion des nouvelles associations
                        string queryRecetteCategorie = "INSERT INTO categories_recettes(id_categorie, id_recette) VALUES(@id_categorie, @id_recette)";

                        List<object> list = new List<object>();
                        foreach (int categorie_id in recette.categories_ids)
                        {
                            list.Add(new { id_recette = recette.id, id_categorie = categorie_id });
                        }

                        // Exécution de l’insertion des catégories
                        res = connexion.Execute(queryRecetteCategorie, list);

                        if (res != recette.categories_ids.Count)
                        {
                            // Annule si toutes les catégories ne sont pas insérées
                            if (anciennePhoto != null)
                            {
                                System.IO.File.Delete("wwwroot" + recette.photo);
                            }
                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }

                        // Si tout s’est bien passé, on valide la transaction
                        transaction.Commit();

                        // Supprime l’ancienne photo si une nouvelle a été enregistrée
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + anciennePhoto);
                        }
                    }
                }
            }

            // Redirection vers l'index après succès
            return RedirectToAction("Index");
        }
        catch (System.Exception e)
        {
            // TODO : Gérer la réaffichage du formulaire avec les erreurs
            return View();
        }
    }

    [HttpPost] // L'action répond à une requête POST (sécurité renforcée pour les suppressions)
    [ValidateAntiForgeryToken] // Protection CSRF
    [Authorize(Roles = "admin")] // Seuls les utilisateurs avec le rôle "admin" peuvent supprimer une recette
    public IActionResult Supprimer([FromForm] int idRecette)
    {
        // Requête SQL pour supprimer une recette par son id
        string query = "DELETE FROM recettes WHERE id=@id";

        try
        {
            // Connexion à la base PostgreSQL
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                // Exécution de la requête avec le paramètre
                int res = connexion.Execute(query, new { id = idRecette });

                // Vérifie si une ligne a bien été supprimée
                if (res == 1)
                {
                    // Message de succès stocké temporairement (affichable dans la vue)
                    TempData["ValidateMessage"] = "Le recette a bien été supprimé";
                }
                else
                {
                    // Si aucune ligne supprimée, on lève une exception
                    throw new Exception();
                }
            }
        }
        catch (Exception e)
        {
            // En cas d'erreur, message d'échec stocké pour affichage
            TempData["ValidateMessage"] = "Erreur, la recette n'a pas pu être supprimé.";
        }

        // Redirige vers la page d’index des recettes
        return RedirectToAction("Index");
    }

    //public IActionResult Recherche2(string categorie)
    //{
    //    ViewBag.Categorie = categorie;
    //    return View();
    //}

    // Attribut qui limite l'accès à l'action aux utilisateurs authentifiés
    [Authorize]

    // Action du contrôleur qui retourne la vue de la page de recherche
    public IActionResult PageRecherche()
    {
        // Retourne la vue nommée "Recherche" (fichier Recherche.cshtml)
        return View("Recherche");
    }

    // Attribut qui restreint l'accès à l'action aux utilisateurs authentifiés
    [Authorize]

    // Action du contrôleur qui répond à une requête de recherche
    public IActionResult Recherche(string recherche)
    {
        // Si la chaîne de recherche est vide ou ne contient que des espaces, retourne une liste vide au format JSON
        if (string.IsNullOrWhiteSpace(recherche))
            return Json(new List<Recette>());

        // Requête SQL : sélectionne toutes les recettes dont le nom correspond (partiellement, sans tenir compte de la casse) à la chaîne de recherche
        string query = "SELECT * FROM Recettes WHERE lower(nom) LIKE @recherche";

        // Déclare une liste qui contiendra les résultats des recettes trouvées
        List<Recette> recettes;

        // Ouvre une connexion à la base de données PostgreSQL
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            // Exécute la requête SQL en injectant la valeur recherchée (en minuscules et entourée de %) pour faire une recherche partielle
            recettes = connexion.Query<Recette>(query, new { recherche = $"%{recherche.ToLower()}%" }).ToList();
        }

        // Retourne la liste des recettes trouvées sous forme de JSON
        return Json(recettes);
    }

    [Authorize] // Seuls les utilisateurs authentifiés peuvent accéder à cette action
    public IActionResult MesRecettes()
    {
        // Récupère l'ID de l'utilisateur connecté à partir de son identité (claim)
        string? idUtilisateur = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Si aucun utilisateur n'est connecté, renvoie un statut 401 (non autorisé)
        if (string.IsNullOrEmpty(idUtilisateur))
        {
            return Unauthorized();
        }

        // Requête SQL pour récupérer les recettes créées par l'utilisateur connecté
        string query = "SELECT * FROM Recettes WHERE createur = @createur";

        List<Recette> recettes; // Liste des recettes à afficher

        // Connexion à la base PostgreSQL
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            // Exécute la requête SQL avec l'ID de l'utilisateur
            recettes = connexion.Query<Recette>(query, new { createur = int.Parse(idUtilisateur) }).ToList();
        }

        // Retourne la vue associée avec les recettes de l'utilisateur
        return View(recettes);
    }
}

