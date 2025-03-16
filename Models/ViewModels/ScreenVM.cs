using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.ViewModels
{
    public class ScreenVM
    {
        [ValidateNever]
        public Theater Theater { get; set; }
        public Screen Screen { get; set; }
    }
}
