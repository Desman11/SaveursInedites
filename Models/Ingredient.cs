namespace SaveursInedites.Models
{
    // La classe Ingredient représente un ingrédient utilisé dans les recettes.
    public class Ingredient
    {
        // Identifiant unique de l'ingrédient dans la base de données.
        // C'est la clé primaire de la table 'Ingredients'.
        public int Id { get; set; }

        // Nom de l'ingrédient. Il est défini comme une chaîne vide par défaut pour éviter les valeurs nulles.
        // Ce nom est utilisé pour identifier un ingrédient dans l'application.
        public string Nom { get; set; } = string.Empty;
    }
}
