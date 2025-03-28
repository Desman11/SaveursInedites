using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using SaveursInedites.Models;
using Microsoft.Extensions.Logging;

namespace SaveursInedites.Controllers
{

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
        
        /// <summary>
        /// Action permettant d'afficher toutes les recettes
        /// </summary>
        /// <returns>la vue html affichant toutes les recttes</returns>
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
                    recette.Ingredients = connexion.Query<Ingredient>(queryIngredients, new { id }).ToList();
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
        // retourne le formulaire permettant de créer un livre
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
                    // TODO renvoyer la list de selectlistitem
                    // if (ModelState["recette.nom"].ValidationState == ModelValidationState.Invalid)
                    // {
                    //     ViewData["ValidateMessage"] = "Le nom n'est pas valide.";
                    // }
                    // ça va pas => on déclenche une exeption
                    throw new Exception("Le modèle est pas valide");

                }
                string query = "INSERT INTO Recettes (nom, temps_preparation, temps_cuisson, difficulte,photo) VALUES(@nom,@temps_preparation,@temps_cuisson,@difficulte,@photo) RETURNING id;";
                string queryRecetteCategorie = "INSERT INTO categories_recettes(id_recette, id_categorie) VALUES(@id_recette, @id_categorie)";
                //  gestion de la couverture

                if (recette.couvertureFile != null && recette.couvertureFile.Length > 0)
                {
                    filePath = Path.Combine("/images/recettes/",
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.couvertureFile.FileName)).ToString();

                    using (var stream = System.IO.File.Create("wwwroot" + filePath))
                    {
                        recette.couvertureFile.CopyTo(stream);
                    }
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
                            transaction.Rollback();
                            throw new Exception("Erreur pendant la création d'une recette.");
                        }
                        else
                        {
                            // ajouter les liens avec les catégories
                            List<object> list = new List<object>();
                            foreach (int id_categorie in recette.categories_ids)
                            {
                                list.Add(new { id_recette = recette_id, id_categorie = id_categorie });
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


    }

}








