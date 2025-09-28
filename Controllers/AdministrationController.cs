using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin,Super Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Super Admin")]
        public IActionResult ListOfRoles()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult ListOfUsers()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        [Authorize(Policy = "CanCreateRolePolicy")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CanCreateRolePolicy")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel createRoleViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = createRoleViewModel.roleName
                };

                IdentityResult result = await _roleManager.CreateAsync(role);

                if (result.Succeeded) {
                    return RedirectToAction("ListOfRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(createRoleViewModel);
        }

        [HttpGet]
        [Authorize(Policy = "CanEditRolePolicy")]
        public async Task<IActionResult> EditRole(string id) 
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel { 
                Id = role.Id,
                roleName = role.Name
            };

            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "CanEditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel editRoleViewModel)
        {
            var role = await _roleManager.FindByIdAsync(editRoleViewModel.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {editRoleViewModel.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = editRoleViewModel.roleName;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListOfRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(editRoleViewModel);
            }            
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.RoleId = roleId;
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            var users = _userManager.Users.ToList();

            foreach (var user in users) 
            {
                var userRoleViewModel = new UserRoleViewModel 
                { 
                   UserId = user.Id,
                   UserName = user.UserName
                };

                if(await _userManager.IsInRoleAsync(user, role.Name))
                    userRoleViewModel.IsSelected = true;
                else
                    userRoleViewModel.IsSelected = false;

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]        
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> userRoleViewModels,string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            IdentityResult? identityResult = null;

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < userRoleViewModels.Count; i++)
            {
                var user = await  _userManager.FindByIdAsync(userRoleViewModels[i].UserId);                

                if (userRoleViewModels[i].IsSelected && !await _userManager.IsInRoleAsync(user, role.Name))
                    identityResult = await _userManager.AddToRoleAsync(user, role.Name);
                else if (!userRoleViewModels[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                    identityResult = await _userManager.RemoveFromRoleAsync(user, role.Name);
                else
                    continue;

                if(i < (userRoleViewModels.Count - 1)) 
                    continue;
                else
                    return RedirectToAction("EditRole", new {id = roleId});
            }

            return RedirectToAction("EditRole", new { id = roleId });
        }

        [HttpGet]
        [Authorize(Policy = "CanEditUserPolicy")]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} cannot be found";
                return View("NotFound");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel { 
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Claims = userClaims.Select(x => x.Value).ToList(),
                UserRoles = userRoles,
                NormalizedUserName = user.NormalizedUserName
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "CanEditUserPolicy")]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.NormalizedUserName = model.NormalizedUserName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return RedirectToAction("ListOfUsers");

                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [Authorize(Policy = "CanDeleteUserPolicy")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return RedirectToAction("ListOfUsers");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListOfUsers");
            }
        }

        [Authorize(Policy = "CanDeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                    return RedirectToAction("ListOfRoles");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListOfRoles");
            }
        }

        [HttpGet]
        [Authorize(Policy = "CannotSelfEditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.UserId = userId;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();
            var roles = _roleManager.Roles.ToList();

            foreach (var role in roles) 
            {
                var userRoleViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty
                };

                if (await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty))
                    userRoleViewModel.IsSelected = true;
                else
                    userRoleViewModel.IsSelected = false;

                model.Add(userRoleViewModel);   
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "CannotSelfEditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> userRoleViewModels, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} cannot be found";
                return View("NotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded) {
                ModelState.AddModelError("", "Cannot Remove User Existing Role");
                return View(userRoleViewModels);
            }

            result = await _userManager.AddToRolesAsync(user, userRoleViewModels.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded) {
                ModelState.AddModelError("", "Cannot add selected role to User");
                return View(userRoleViewModels);
            }

            return RedirectToAction("EditUser", new { userId = userId });
        }

        [HttpGet]
        [Authorize(Policy = "CannotSelfEditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} cannot be found";
                return View("NotFound");
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId,
            };

            foreach (Claim claim in ClaimStore.AllClaims) {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                };

                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "CannotSelfEditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {model.UserId} cannot be found";
                return View("NotFound");
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            result = await _userManager.AddClaimsAsync(user, model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to the user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { userId = model.UserId});
        }

        /*[HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }*/
    }
}
