namespace SaveursInedites.Models
{
    // La classe Etape représente une étape dans la recette.
    // Chaque étape contient un numéro d'ordre et un texte décrivant l'action à réaliser à cette étape.
    // Ces informations sont utilisées pour détailler les instructions de préparation de la recette.

    public class Etape
    {
        // Le numéro de l'étape dans la recette (ex : étape 1, étape 2, etc.).
        // Ce champ est utilisé pour ordonner les étapes dans la recette, indiquant l'ordre des instructions.
        // Il est généralement un entier, et chaque étape doit avoir un numéro unique dans le contexte de la recette.
        public int numero { get; set; }

        // Le texte qui décrit l'action à réaliser à cette étape de la recette.
        // Ce champ contient les instructions de préparation spécifiques à chaque étape.
        // Il est initialisé à une chaîne vide afin d'éviter les valeurs nulles.
        public string texte { get; set; } = string.Empty;
    }
}
