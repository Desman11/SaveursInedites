
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SaveursInedites.Models
{
    public class Recette
    {
        public int id { get; set; }

        [DisplayName("Nom")]
        [Required(ErrorMessage = "Le nom est requise.")]
        [MaxLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
        public string nom { get; set; } = string.Empty;

        [DataType(DataType.Time)]
        [DisplayName("Temps de préparation")]
        [Required(ErrorMessage = "Le temps de préparation est requise.")]
        public TimeSpan temps_preparation { get; set; }

        [DisplayName("Temps de cuisson")]
        public TimeSpan temps_cuisson { get; set; } = TimeSpan.Zero;
        [Required(ErrorMessage = "La difficulté est requise.")]

        public int difficulte { get; set; }

        public string? photo { get; set; } // pour l'affichage

        public List<Etape> Etapes { get; set; } = new List<Etape>();
        
        public List<Categorie> categories { get; set; } = new List<Categorie>(); // utilisé pour l'affichage        
        [DisplayName("Catégories")]
        [Required(ErrorMessage = "La catégorie est requise.")]

        public List<int> categories_ids { get; set; } = new List<int>(); // utilisé pour la création ou l'édition
        

        [DisplayName("Photo")]
        public IFormFile? photoFile { get; set; } // pour le formulaire
                                                      // Liste des ingrédients liés à la recette
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();



    }
}




