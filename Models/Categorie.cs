namespace SaveursInedites.Models
{
    // La classe Categorie représente une catégorie de recettes dans l'application.
    // Chaque catégorie permet de regrouper des recettes similaires sous une même étiquette.
    public class Categorie
    {
        // Propriété "Id" qui est l'identifiant unique de chaque catégorie.
        // Il est utilisé pour faire référence à une catégorie spécifique dans la base de données.
        public int Id { get; set; }

        // Propriété "Nom" qui contient le nom de la catégorie (par exemple : "Dessert", "Entrée", etc.).
        // Cela sert à décrire la catégorie et à l'afficher dans l'interface utilisateur.
        public string? Nom { get; set; }
    }
}

