using System.ComponentModel.DataAnnotations;   // Pour les attributs de validation comme [Required], [MaxLength], etc.
using System.ComponentModel;                  // Pour l'attribut [DisplayName]

namespace SaveursInedites.Models
{
    // Représente une recette complète dans l'application Saveurs Inédites
    public class Recette
    {
        // Identifiant unique de la recette
        public int id { get; set; }

        // Nom de la recette avec un affichage personnalisé et validation
        [DisplayName("Nom")]
        [Required(ErrorMessage = "Le nom est requise.")]
        [MaxLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
        public string nom { get; set; } = string.Empty;

        // Temps de préparation de la recette (champ requis, affiché sous forme d'heure)
        [DataType(DataType.Time)]
        [DisplayName("Temps de préparation")]
        [Required(ErrorMessage = "Le temps de préparation est requise.")]
        public TimeSpan temps_preparation { get; set; }

        // Temps de cuisson (optionnel, par défaut à zéro)
        [DisplayName("Temps de cuisson")]
        public TimeSpan temps_cuisson { get; set; } = TimeSpan.Zero;

        // Niveau de difficulté de la recette (champ requis)
        [Required(ErrorMessage = "La difficulté est requise.")]
        public int difficulte { get; set; }

        // Chemin ou URL de la photo à afficher (optionnel)
        public string? photo { get; set; } // pour l'affichage

        // Liste des étapes de la recette (liée à un autre modèle)
        public List<Etape> Etapes { get; set; } = new List<Etape>();

        // Liste des catégories (utilisée pour l'affichage uniquement)
        public List<Categorie> categories { get; set; } = new List<Categorie>();

        // Identifiants des catégories sélectionnées (pour la création/édition)
        [DisplayName("Catégories")]
        [Required(ErrorMessage = "La catégorie est requise.")]
        public List<int> categories_ids { get; set; } = new List<int>();

        // Fichier image uploadé via formulaire (non stocké en base)
        [DisplayName("Photo")]
        public IFormFile? photoFile { get; set; } // pour le formulaire

        // Liste des ingrédients associés à la recette
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        // Identifiant du créateur de la recette (lié à l'utilisateur)
        public int Createur { get; set; }
    }
}
