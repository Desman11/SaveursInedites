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


namespace SaveursInedites.Controllers;

[Authorize]
public class RecettesController : Controller
{
    private readonly string _connexionString;

    public RecettesController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        // récupération de la chaîne de connexion dans la configuration
        _connexionString = configuration.GetConnectionString("Saveurs_Inedites")!;
        // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
        if (_connexionString == null)
        {
            throw new Exception("Error : Connexion string not found ! ");
        }
    }
    [Authorize]
    public IActionResult Index()
    {
        string query = "SELECT * FROM Recettes";
        List<Recette> recettes;
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            recettes = connexion.Query<Recette>(query).ToList();
        }
        return View(recettes);
    }

    [Authorize]
    public IActionResult Detail(int id)
    {
        string queryRecette = "SELECT * FROM recettes WHERE id = @id";
        string queryEtapes = "SELECT numero, texte FROM etapes WHERE id_recette = @id ORDER BY numero";
        string queryIngredients = "SELECT i.* FROM ingredients i JOIN ingredients_recettes ir ON i.id = ir.id_ingredient WHERE ir.id_recette = @id";
        string queryAvis = "SELECT note, commentaire FROM avis WHERE id_recette = @id";
        string queryMoyenne = "SELECT AVG(note) FROM avis WHERE id_recette = @id";

        Recette recette;

        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            try
            {
                connexion.Open();

                // Récupération de la recette principale
                recette = connexion.QuerySingleOrDefault<Recette>(queryRecette, new { id });
                if (recette == null)
                    return NotFound();

                // Étapes
                recette.Etapes = connexion.Query<Etape>(queryEtapes, new { id }).ToList();

                // Ingrédients
                recette.Ingredients = connexion.Query<Ingredient>(queryIngredients, new { id }).ToList();

                // Avis
                var avisListe = connexion.Query<AvisViewModel>(queryAvis, new { id }).ToList();

                // Moyenne
                double? moyenne = connexion.ExecuteScalar<double?>(queryMoyenne, new { id });


                // Création du ViewModel complet
                var model = new RecetteAvisViewModel
                {
                    Recette = recette,
                    NouvelAvis = new AvisViewModel { id_recette = id },
                    AvisExistants = avisListe,
                    MoyenneNote = moyenne ?? 0
                };

                ViewData["titrePage"] = recette.nom;
                ViewData["Moyenne"] = moyenne;
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des détails de la recette : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur");
            }
        }
    }

    //public IActionResult Detail(int id)
    //{
    //    string query = "SELECT * FROM recettes WHERE id = @id";
    //    string queryEtapes = "SELECT numero, texte FROM etapes WHERE id_recette = @id ORDER BY numero";
    //    string queryIngredients = "SELECT * FROM recettes WHERE id = @id; SELECT i.* FROM ingredients i JOIN ingredients_recettes ir ON i.id = ir.id_ingredient WHERE ir.id_recette = @id; ";
    //    string queryAvis = "SELECT note, commentaire FROM avis WHERE id_recette = @id";
    //    string queryMoyenne = "SELECT AVG(note) FROM avis WHERE id_recette = @id";
    //    Recette recette;

    //    using (var connexion = new NpgsqlConnection(_connexionString))
    //    {
    //        try
    //        {
    //            connexion.Open();

    //            recette = connexion.QuerySingleOrDefault<Recette>(query, new { id });

    //            if (recette == null)
    //            {
    //                return NotFound();
    //            }

    //            recette.Etapes = connexion.Query<Etape>(queryEtapes, new { id }).ToList();
    //            using (var multi = connexion.QueryMultiple(queryIngredients, new { id }))
    //            {
    //                recette = multi.Read<Recette>().SingleOrDefault();
    //                recette.Ingredients = multi.Read<Ingredient>().ToList();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Erreur lors de la récupération des détails de la recette : {ex.Message}");
    //            return StatusCode(500, "Erreur interne du serveur");
    //        }
    //    }

    //    //ViewData["titrePage"] = recette.nom;
    //    //return View(recette);
    //    var model = new RecetteAvisViewModel
    //    {
    //        Recette = recette,
    //        NouvelAvis = new AvisViewModel { RecetteId = id }
    //    };

    //    ViewData["titrePage"] = recette.nom;
    //    return View(model);
    //}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjouterAvis([FromForm] RecetteAvisViewModel model)
    {
        var avis = model.NouvelAvis;


        ModelState["Recette"].ValidationState = ModelValidationState.Skipped;
        ModelState["AvisExistants"].ValidationState = ModelValidationState.Skipped;

        if (!ModelState.IsValid || avis.Note < 1 || avis.Note > 5 )
        {
            TempData["Message"] = "Veuillez remplir tous les champs correctement.";
            return RedirectToAction("Detail", "Recettes", new { id = avis.id_recette });
        }

        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            string query = "INSERT INTO Avis (id_recette,id_utilisateur, note, commentaire) VALUES (@id_recette,@id_utilisateur, @note, @commentaire)";
            connexion.Execute(query, new
            {
                id_recette = avis.id_recette,
                id_utilisateur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                note = avis.Note,
                commentaire = avis.Commentaire

            });
        }

        TempData["Message"] = "Merci pour votre avis !";
        return RedirectToAction("Detail", "Recettes", new { id = avis.id_recette });
    }



    private List<SelectListItem> ConstruireListeCategories()
    {
        // récupération de toutes les catégories
        string queryCategories = "SELECT * FROM Categories";
        List<Categorie> categories;
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            categories = connexion.Query<Categorie>(queryCategories).ToList();
        }
        List<SelectListItem> liste_cat = new List<SelectListItem>();
        foreach (Categorie categorie in categories)
        {
            liste_cat.Add(new SelectListItem(categorie.Nom, categorie.Id.ToString()));
        }
        return liste_cat;
    }

    // retourne le formulaire permettant de créer une recette
    [HttpGet]
    public IActionResult Nouveau()
    {
        EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();

        viewModel.categories = ConstruireListeCategories();

        ViewData["ActionFormulaire"] = "Nouveau";
        // retourne la vue spécifiée (qui est dans le dossier Recettes)
        return View("EditeurRecette", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nouveau([FromForm] Recette recette)
    {

        string? filePath = null;
        string? anciennePhoto = null;
        try
        {

            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle est pas valide");
            }
            string[] permittedExtensions = { ".jpeg", ".jpg", ".png", ".gif" };

            var ext = Path.GetExtension(recette.photoFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {

                ModelState["recette.photoFile"]!.Errors.Add(new ModelError("Ce type de fichier n'est pas accepté."));
                throw new Exception("Ce type de fichier n'est pas accepté.");
            }


            string query = "INSERT INTO Recettes (nom, temps_preparation, temps_cuisson, difficulte,photo) VALUES(@nom,@temps_preparation,@temps_cuisson,@difficulte,@photo) RETURNING id;";
            string queryRecetteCategorie = "INSERT INTO categories_recettes(id_categorie,id_recette) VALUES(@id_categorie,@id_recette)";

            //gestion de la photo



            if (recette.photoFile != null && recette.photoFile.Length > 0)
            {
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.photoFile.FileName)).ToString();

                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.photoFile.CopyTo(stream);
                }
                // récupération du lien de l'ancienne photo
                anciennePhoto = recette.photo;
                recette.photo = filePath;
            }

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var transaction = connexion.BeginTransaction())
                {
                    // insert d'une recette et récupération de son id
                    int recette_id = connexion.ExecuteScalar<int>(query, recette);
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
                        // ajouter les liens avec les catégories
                        List<object> list = new List<object>();
                        foreach (int categorie_id in recette.categories_ids)
                        {
                            list.Add(new { id_recette = recette_id, id_categorie = categorie_id });
                        }
                        int res = connexion.Execute(queryRecetteCategorie, list);
                        if (res != recette.categories_ids.Count)
                        {
                            if (anciennePhoto != null)
                            {
                                System.IO.File.Delete("wwwroot" + recette.photo);
                            }
                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }
                        transaction.Commit();
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + anciennePhoto);
                        }
                    }
                }
            }
        }


        catch (Exception e)
        {
            if (filePath != null)
            {
                System.IO.File.Delete("wwwroot" + filePath);
            }
            ViewData["ValidateMessage"] = e.Message;
            EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();
            viewModel.recette = recette;
            viewModel.categories = ConstruireListeCategories();
            return View("EditeurRecette", viewModel);
        }
        TempData["ValidateMessage"] = "La recette a bien été créé";
        return RedirectToAction("Index");
    }
    //[Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult Modifier(int id)
    {

        EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();

        viewModel.categories = ConstruireListeCategories();

        string queryRecette = "SELECT * FROM Recettes WHERE recettes.id=@identifiant";
        string queryCategories = "SELECT id_categorie FROM categories_recettes WHERE id_recette=@identifiant";


        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            try
            {
                viewModel.recette = connexion.QueryFirst<Recette>(queryRecette, new { identifiant = id });
                viewModel.recette.categories_ids = connexion.Query<int>(queryCategories, new { identifiant = id }).ToList();
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
        
        ViewData["ActionFormulaire"] = "Modifier";
        // retourne la vue spécifiée (qui est dans le dossier recettes)
        if (viewModel.recette.photo != null)
        {
            ViewData["AfficherPhoto"] = true;
        }
        else
        {
            ViewData["AfficherPhoto"] = false;
        }

        return View("EditeurRecette", viewModel);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[EstCreateurAuthorize]
    // [Authorize(Roles = "admin")]
    public IActionResult Modifier([FromForm] Recette recette, [FromForm] string supprimerPhoto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle est pas valide");
            }

            // gestion de la photo
            string? filePath = null;
            string? anciennePhoto = null;
            if (recette.photoFile != null && recette.photoFile.Length > 0)
            {
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.photoFile.FileName)).ToString();

                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.photoFile.CopyTo(stream);
                }

                // récupération du lien de l'ancienne photo
                anciennePhoto = recette.photo;
                // assignation de la nouvelle photo
                recette.photo = filePath;
            }

            string query = "UPDATE recettes SET nom=@nom, temps_preparation=@temps_preparation, temps_cuisson=@temps_cuisson, difficulte=@difficulte, photo=@photo  WHERE id=@id";

       


            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var transaction = connexion.BeginTransaction())
                {
                    // mise à jour de la recette sans prendre en compte les catgéories
                    int res = connexion.Execute(query, recette);
                    if (res != 1)
                    {
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + recette.photo);
                        }
                        transaction.Rollback();
                        throw new Exception("Erreur pendant la mise à jour de al recette");
                    }
                    else
                    {
                        // mise à jour des catégories
                        // 1 - suppression des catégories
                        string queryDelete = "DELETE FROM categories_recettes WHERE id_recette=@identifiant";
                        connexion.Execute(queryDelete, new { identifiant = recette.id });
                        // 2 - insertion des catégories
                        string queryRecetteCategorie = "INSERT INTO categories_recettes(id_categorie, id_recette) VALUES(@id_categorie, @id_recette)";

                       
                        List<object> list = new List<object>();
                        foreach (int categorie_id in recette.categories_ids)
                        {
                            list.Add(new { id_recette = recette.id, id_categorie = categorie_id });
                        }
                        res = connexion.Execute(queryRecetteCategorie, list);



                        if (res != recette.categories_ids.Count)
                        {
                            if (anciennePhoto != null)
                            {
                                System.IO.File.Delete("wwwroot" + recette.photo);
                            }
                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }
                        transaction.Commit();
                        if (anciennePhoto != null)
                        {
                            System.IO.File.Delete("wwwroot" + anciennePhoto);
                        }
                    }

                }
            }
            // TODO message de modification bien effectuée
            return RedirectToAction("Index");
        }
        catch (System.Exception e)
        {
            // TODO gérer le renvoie vers le formulaire
            return View();
        }


    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]

    public IActionResult Supprimer([FromForm] int idRecette)
    {
        string query = "DELETE FROM recettes WHERE id=@id";

        try
        {
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                int res = connexion.Execute(query, new { id = idRecette });
                if (res == 1)
                {
                    TempData["ValidateMessage"] = "Le recette a bien été supprimé";
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        catch (Exception e)
        {
            TempData["ValidateMessage"] = "Erreur, la recette n'a pas pu être supprimé.";

        }
        return RedirectToAction("Index");
    }
    //public IActionResult Recherche2(string categorie)
    //{
    //    ViewBag.Categorie = categorie;
    //    return View();
    //}

    [Authorize]
    public IActionResult PageRecherche()
    {
        return View("Recherche");
    }

    [Authorize]
    public IActionResult Recherche(string recherche)
    {
        if (string.IsNullOrWhiteSpace(recherche))
            return Json(new List<Recette>());

        string query = "SELECT * FROM Recettes WHERE lower(nom) LIKE @recherche";
        List<Recette> recettes;
        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            recettes = connexion.Query<Recette>(query, new { recherche = $"%{recherche.ToLower()}%" }).ToList();
        }
        return Json(recettes);
    }
}

