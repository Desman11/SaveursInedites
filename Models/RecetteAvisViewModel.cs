using SaveursInedites.Models;

public class RecetteAvisViewModel
{
    public Recette Recette { get; set; } // La recette principale

    public AvisViewModel NouvelAvis { get; set; } // L'avis que l'utilisateur veut soumettre

    public List<AvisViewModel> AvisExistants { get; set; } // Liste des avis existants

    public double MoyenneNote { get; set; } // Moyenne des notes
}

