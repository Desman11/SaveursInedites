namespace SaveursInedites.Models
{
    // La classe ErrorViewModel est utilisée pour représenter les données relatives à une erreur
    // qui se produit dans l'application. Elle est utilisée principalement pour afficher des informations
    // sur l'erreur, comme un identifiant de requête (RequestId), afin de faciliter le débogage ou l'affichage
    // d'un message d'erreur plus détaillé.

    public class ErrorViewModel
    {
        // La propriété RequestId représente un identifiant unique pour la requête HTTP ayant échoué.
        // Cela peut être utile pour le traçage des erreurs côté serveur ou pour l'analyse des logs.
        public string? RequestId { get; set; }

        // La propriété ShowRequestId est une propriété calculée qui retourne true si RequestId a une valeur
        // non nulle ou non vide. Elle permet de décider si le RequestId doit être affiché dans la vue d'erreur.
        // Cela peut être utilisé pour afficher l'identifiant de la requête dans la page d'erreur si nécessaire.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
