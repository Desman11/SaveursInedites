namespace SaveursInedites.Models
{
    // La classe AvisViewModel représente le modèle de données utilisé pour capturer l'avis d'un utilisateur dans l'interface utilisateur.
    public class AvisViewModel
    {
        // Propriété "id_recette" qui lie cet avis à une recette spécifique
        // Cela permet de savoir à quelle recette l'avis correspond
        public int id_recette { get; set; }

        // Propriété "Note" qui représente la note donnée par l'utilisateur sur la recette
        // La note est un entier compris entre 1 et 5, utilisé pour évaluer la recette
        public int Note { get; set; }

        // Propriété "Commentaire" qui contient le texte du commentaire de l'utilisateur concernant la recette
        // L'utilisateur peut ajouter des informations supplémentaires ou une critique détaillée ici
        public string Commentaire { get; set; }
    }
}
