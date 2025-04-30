using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SaveursInedites.Models;

public class Utilisateur
{
    // Identifiant unique de l'utilisateur (clé primaire)
    public int Id { get; set; }

    // Identifiant de l'utilisateur : requis, longueur minimale de 1 et maximale de 20 caractères, texte
    [Required(ErrorMessage = "L'identifiant est requis")]
    [MinLength(1)] // Le nom de l'utilisateur ne peut pas être vide
    [MaxLength(20)] // Limite la longueur de l'identifiant à 20 caractères
    [DataType(DataType.Text)] // Définit que l'identifiant est un texte
    public string identifiant { get; set; } = string.Empty;

    // Email de l'utilisateur : requis et doit être une adresse email valide
    [Required(ErrorMessage = "L'email est requis.")]
    [DataType(DataType.EmailAddress)] // Validation que l'email est valide
    public string? email { get; set; }

    // Mot de passe de l'utilisateur : requis, et de type mot de passe (masqué dans l'interface)
    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)] // Masque le mot de passe dans l'interface
    public string? password { get; set; }

    // Le rôle attribué à l'utilisateur, lié à la classe `Role`
    public Role? role { get; set; }

    // Identifiant du rôle de l'utilisateur : requis
    [Required(ErrorMessage = "Le rôle est requis")]
    public int role_id { get; set; }
}
