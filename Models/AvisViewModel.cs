namespace SaveursInedites.Models
{
    public class AvisViewModel
    {
        public int id_recette { get; set; } // Associe l'avis à une recette
        public int Note { get; set; } // Note entre 1 et 5
        public string Commentaire { get; set; } // Commentaire de l'avis
    }
}



