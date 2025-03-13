using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.ViewModels;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDBContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RoleManagement(string? id)
        {
            if (id == null || id == "")
            {
                return NotFound();
            }

            // get the user by id
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // get all the roles
            var roles = _db.Roles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Name
            });

            // get the role of user
            string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;

            user.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            // create the view model
            var userRoleUpdateVM = new UserRoleUpdateVM
            {
                User = user,
                AvailableRoles = roles,
                SelectedRole = user.Role
            };


            return View(userRoleUpdateVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(UserRoleUpdateVM userRoleUpdateVM)
        {

            // get the role id
            string oldRoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userRoleUpdateVM.User.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == oldRoleId).Name;

            // update the role
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userRoleUpdateVM.User.Id);
            _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, userRoleUpdateVM.SelectedRole).GetAwaiter().GetResult();

            TempData["Success"] = "Role updated successfully";
            return RedirectToAction(nameof(Index));
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _db.ApplicationUsers.ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked, we will unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }


}
