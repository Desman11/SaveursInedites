using Microsoft.AspNetCore.Mvc.Rendering;

namespace SaveursInedites.Models
{
    // La classe EditeurRecetteViewModel représente le modèle utilisé pour l'édition ou la création d'une recette.
    // Ce modèle contient la recette à éditer ou ajouter, ainsi que la liste des catégories disponibles
    // pour lier une recette à une catégorie spécifique.

    public class EditeurRecetteViewModel
    {
        // Propriété "recette" qui contient les informations de la recette à éditer ou ajouter.
        // Cela inclut des propriétés comme le nom, la description, les ingrédients, etc.
        public Recette recette { get; set; }

        // Propriété "categories" qui est une liste d'éléments SelectListItem représentant les catégories disponibles
        // dans l'application. Cette liste est utilisée pour afficher un contrôle de sélection de catégorie
        // lors de l'édition ou de la création d'une recette.
        public List<SelectListItem> categories { get; set; } = new List<SelectListItem>();

    }
}


