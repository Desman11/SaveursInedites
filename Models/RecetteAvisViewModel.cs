using SaveursInedites.Models;

public class RecetteAvisViewModel
{
    // La recette pour laquelle les avis sont collectés.
    // Cette propriété contient l'objet Recette principal qui est affiché sur la page.
    public Recette Recette { get; set; }

    // L'avis que l'utilisateur souhaite soumettre pour la recette.
    // Il est encapsulé dans un AvisViewModel qui contient la note et le commentaire de l'utilisateur.
    public AvisViewModel NouvelAvis { get; set; }

    // Liste des avis déjà existants pour cette recette.
    // Cette propriété contient un ensemble d'avis existants, sous forme de AvisViewModel, qui ont été soumis par d'autres utilisateurs.
    public List<AvisViewModel> AvisExistants { get; set; }

    // La moyenne des notes attribuées à la recette par tous les utilisateurs.
    // Cette propriété calcule la note moyenne à partir des notes des avis existants.
    public double MoyenneNote { get; set; }
}
