using System.ComponentModel.DataAnnotations;

namespace SaveursInedites.Models
{
    public class Role
    {
        // Clé primaire pour l'entité Role
        public int id { get; set; }

        // Nom du rôle, requis, avec une longueur minimale de 1 et maximale de 20 caractères
        // Le nom du rôle doit être un texte
        [Required(ErrorMessage = "Le role est requis")]
        [MinLength(1)] // Le nom du rôle doit avoir au moins 1 caractère
        [MaxLength(20)] // Le nom du rôle ne peut pas dépasser 20 caractères
        [DataType(DataType.Text)] // Définit que ce champ est de type texte
        public string nom { get; set; }
    }
}
