namespace SaveursInedites.Models
{
    // La classe ErrorViewModel est utilis�e pour repr�senter les donn�es relatives � une erreur
    // qui se produit dans l'application. Elle est utilis�e principalement pour afficher des informations
    // sur l'erreur, comme un identifiant de requ�te (RequestId), afin de faciliter le d�bogage ou l'affichage
    // d'un message d'erreur plus d�taill�.

    public class ErrorViewModel
    {
        // La propri�t� RequestId repr�sente un identifiant unique pour la requ�te HTTP ayant �chou�.
        // Cela peut �tre utile pour le tra�age des erreurs c�t� serveur ou pour l'analyse des logs.
        public string? RequestId { get; set; }

        // La propri�t� ShowRequestId est une propri�t� calcul�e qui retourne true si RequestId a une valeur
        // non nulle ou non vide. Elle permet de d�cider si le RequestId doit �tre affich� dans la vue d'erreur.
        // Cela peut �tre utilis� pour afficher l'identifiant de la requ�te dans la page d'erreur si n�cessaire.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
