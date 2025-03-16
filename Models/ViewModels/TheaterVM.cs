using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class TheaterVM
    {
        [ValidateNever]
        public IEnumerable<SelectListItem> Provinces { get; set; }
        public Theater Theater { get; set; }
    }
}
