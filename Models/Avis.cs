namespace SaveursInedites.Models
{
    // La classe Avis représente un avis donné par un utilisateur pour une recette
    public class Avis
    {
        // Propriété "id" qui est en lecture seule (pas modifiable après la création de l'avis)
        public int id { get; }

        // Propriété "note" qui représente la note donnée à la recette (sur une échelle de 1 à 5, par exemple)
        // La propriété peut être nulle (int? signifie qu'elle est nullable)
        public int? note { get; set; }

        // Propriété "commentaire" qui contient le commentaire écrit par l'utilisateur concernant la recette
        // Cette propriété peut aussi être nulle, indiquant qu'il n'y a pas de commentaire laissé
        public string? commentaire { get; set; }
    }
}

