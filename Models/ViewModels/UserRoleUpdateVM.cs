using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class UserRoleUpdateVM
    {
        public ApplicationUser User { get; set; }

        [Required]
        public string SelectedRole { get; set; }

        public IEnumerable<SelectListItem> AvailableRoles { get; set; }
    }
}
