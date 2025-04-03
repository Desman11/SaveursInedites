using System.ComponentModel.DataAnnotations;

namespace SaveursInedites.Models
{
    public class Role
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Le role est requis")]
        [MinLength(1)]
        [MaxLength(20)]
        [DataType(DataType.Text)]
        public string nom { get; set; }

    }
}
