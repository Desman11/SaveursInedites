using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SaveursInedites.Models;

    public class Utilisateur
    {
        public int Id { get; }
        public string identifiant { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [DataType(DataType.EmailAddress)]
        public string? email { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
        public string? password { get; set; }


    //[Required(ErrorMessage = "Le nom est requis.")]
    //[MinLength(1)]
    //[MaxLength(100)]
    //[DataType(DataType.Text)]
    //public string? nom { get; set; }

    //[Required(ErrorMessage = "Le prénom est requis.")]
    //[MinLength(1)]
    //[MaxLength(100)]
    //[DataType(DataType.Text)]
    //public string? prenom { get; set; }

    // public DateTime date_inscription { get; set; }
    //public role? role { get; set; }

    //[Required(ErrorMessage = "Le rôle est requis.")]
    // public int role_id { get; set; }

    // public bool keepLoggedIn {get;set;}
    // TODO Role
}

