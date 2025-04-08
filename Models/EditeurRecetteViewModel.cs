using Microsoft.AspNetCore.Mvc.Rendering;

namespace SaveursInedites.Models
{
    public class EditeurRecetteViewModel
    {
        public Recette recette { get; set; }
        public List<SelectListItem> categories { get; set; } = new List<SelectListItem>();
       }
}

