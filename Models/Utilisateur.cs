using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SaveursInedites.Models;

public class Utilisateur
{
    public int Id { get; set; }

    [Required(ErrorMessage = "L'identifiant est requis")]
    [MinLength(1)]
    [MaxLength(20)]
    [DataType(DataType.Text)]
    public string identifiant { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis.")]
    [DataType(DataType.EmailAddress)]
    public string? email { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
    public string? password { get; set; }


    public Role? role { get; set; }
    [Required(ErrorMessage = "Le rôle est requis")]
    public int role_id { get; set; }
}


