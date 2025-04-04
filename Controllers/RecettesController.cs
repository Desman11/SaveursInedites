using Microsoft.AspNetCore.Mvc;
using SaveursInedites.Models;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SaveursInedites.Controllers;


namespace SaveursInedites.Controllers;

//[Authorize]
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


    public IActionResult Detail(int id)
    {
        string query = "SELECT * FROM recettes WHERE id = @id";
        string queryEtapes = "SELECT numero, texte FROM etapes WHERE id_recette = @id ORDER BY numero";
        string queryIngredients = "SELECT * FROM recettes WHERE id = @id; SELECT i.* FROM ingredients i JOIN ingredients_recettes ir ON i.id = ir.id_ingredient WHERE ir.id_recette = @id; ";

        Recette recette;

        using (var connexion = new NpgsqlConnection(_connexionString))
        {
            try
            {
                connexion.Open();

                recette = connexion.QuerySingleOrDefault<Recette>(query, new { id });

                if (recette == null)
                {
                    return NotFound();
                }

                recette.Etapes = connexion.Query<Etape>(queryEtapes, new { id }).ToList();
                using (var multi = connexion.QueryMultiple(queryIngredients, new { id }))
                {
                    recette = multi.Read<Recette>().SingleOrDefault();
                    recette.Ingredients = multi.Read<Ingredient>().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des détails de la recette : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur");
            }
        }

        ViewData["titrePage"] = recette.nom;
        return View(recette);
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

        // retourne la vue spécifiée (qui est dans le dossier Recettes)
        return View("EditeurRecette", viewModel);
    }

    [HttpPost]
    public IActionResult Nouveau([FromForm] Recette recette)
    {

        string? filePath = null;
        try
        {

            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle est pas valide");
            }
            string[] permittedExtensions = { ".jpeg", ".jpg", ".png", ".gif" };

            var ext = Path.GetExtension(recette.couvertureFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {

                ModelState["recette.couvertureFile"]!.Errors.Add(new ModelError("Ce type de fichier n'est pas accepté."));
                throw new Exception("Ce type de fichier n'est pas accepté.");
            }


            string query = "INSERT INTO Recettes (nom, temps_preparation, temps_cuisson, difficulte,photo) VALUES(@nom,@temps_preparation,@temps_cuisson,@difficulte,@photo) RETURNING id;";
            string queryRecetteCategorie = "INSERT INTO categories_recettes(id_categorie,id_recette) VALUES(@id_categorie,@id_recette)";
            //gestion de la couverture


            if (recette.couvertureFile != null && recette.couvertureFile.Length > 0)
            {
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.couvertureFile.FileName)).ToString();

                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.couvertureFile.CopyTo(stream);
                }
                recette.couverture = filePath;
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
                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }
                        transaction.Commit();
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
        ViewData["TitrePage"] = "Modification recette";
        ViewData["ActionFormulaire"] = "Modifier";
        // retourne la vue spécifiée (qui est dans le dossier recettes)
        return View("EditeurRecette", viewModel);
    }
    //[Authorize(Roles = "admin")]
   //[HttpGet]
   // public IActionResult Modifier(int id)
   // {

   //     EditeurRecetteViewModel viewModel = new EditeurRecetteViewModel();

   //     viewModel.categories = ConstruireListeCategories();

   //     string queryRecette = "SELECT * FROM Recettes WHERE recettes.id=@identifiant";
   //     string queryCategories = "SELECT categorie_id FROM recette_categorie WHERE recette_id=@identifiant";

   //     using (var connexion = new NpgsqlConnection(_connexionString))
   //     {
   //         try
   //         {
   //             viewModel.recette = connexion.QueryFirst<Recette>(queryRecette, new { identifiant = id });
   //             viewModel.recette.categories_ids = connexion.Query<int>(queryCategories, new { identifiant = id }).ToList();
   //         }
   //         catch (Exception e)
   //         {
   //             return NotFound();
   //         }
   //     }
   //     ViewData["TitrePage"] = "Modification recette";
   //     ViewData["ActionFormulaire"] = "Modifier";
   //     // retourne la vue spécifiée (qui est dans le dossier Recettes)
   //     return View("EditeurRecette", viewModel);
   // }

    [HttpPost]
    public IActionResult Modifier([FromForm] Recette recette, [FromForm] string supprimerCouverture)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Le modèle est pas valide");
            }

            // gestion de la couverture
            string? filePath = null;
            string? ancienneCouverture = null;
            if (recette.couvertureFile != null && recette.couvertureFile.Length > 0)
            {
                filePath = Path.Combine("/images/recettes/",
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.couvertureFile.FileName)).ToString();

                using (var stream = System.IO.File.Create("wwwroot" + filePath))
                {
                    recette.couvertureFile.CopyTo(stream);
                }

                // récupération du lien de l'ancienne couverture
                ancienneCouverture = recette.couverture;
                // assignation de la nouvelle couverture
                recette.couverture = filePath;
            }

            string query = "UPDATE recettes SET nom=@nom, temps_preparation=@temps_preparation, temps_cuisson=@temps_cuisson, difficulte=@difficulte, photo=@photo, createur=@createur WHERE id=@id;";


            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                connexion.Open();
                using (var transaction = connexion.BeginTransaction())
                {
                    // mise à jour de la recette sans prendre en compte les catgéories
                    int res = connexion.Execute(query, recette);
                    if (res != 1)
                    {
                        if (ancienneCouverture != null)
                        {
                            System.IO.File.Delete("wwwroot" + recette.couverture);
                        }
                        transaction.Rollback();
                        throw new Exception("Erreur pendant la mise à jour de la recette");
                    }
                    else
                    {
                        // mise à jour des catégories
                        // 1 - suppression des catégories
                        string queryDelete = "DELETE FROM recette_categorie WHERE id_recette=@identifiant";
                        connexion.Execute(queryDelete, new { identifiant = recette.id });
                        // 2 - insertion des catégories
                        string queryRecetteCategorie = "INSERT INTO recette_categorie(id_recette,id_categorie) VALUES(@id_recette,@id_categorie)";
                        List<object> list = new List<object>();
                        foreach (int categorie_id in recette.categories_ids)
                        {
                            list.Add(new { id_recette = recette.id, id_categorie = categorie_id });
                        }
                        res = connexion.Execute(queryRecetteCategorie, list);
                        if (res != recette.categories_ids.Count)
                        {
                            if (ancienneCouverture != null)
                            {
                                System.IO.File.Delete("wwwroot" + recette.couverture);
                            }
                            transaction.Rollback();
                            throw new Exception("Erreur pendant l'ajout des catégories");
                        }
                        transaction.Commit();
                        if (ancienneCouverture != null)
                        {
                            System.IO.File.Delete("wwwroot" + ancienneCouverture);
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
}











