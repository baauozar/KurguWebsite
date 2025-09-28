using System.ComponentModel.DataAnnotations;

namespace KurguWebsite.WebUI.Areas.ViewModel.Service
{
    public class ServiceViewModel
    {
        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        [Display(Name = "Icon URL")]
        public string? IconUrl { get; set; }

        [Display(Name = "Is Featured?")]
        public bool IsFeatured { get; set; }
    }
}
