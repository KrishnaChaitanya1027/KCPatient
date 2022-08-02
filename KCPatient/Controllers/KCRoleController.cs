using KCPatient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCPatient.Controllers
{
    [Authorize(Roles = "administrators")]
    public class KCRoleController : Controller
    {

        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public KCRoleController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var roleIndex = new Roles();
            roleIndex.RolesList = roleManager.Roles.OrderBy(m => m.Name).ToList();
            return View(roleIndex);
        }


        public async Task<IActionResult> AddNewRole(Roles userRole)
        {
            if (string.IsNullOrEmpty(userRole.RoleName))
            {
                TempData["Message"] = "Role name is empty";
                return RedirectToAction("Index");
            }

            userRole.RoleName = userRole.RoleName.Trim();
            var role = await roleManager.FindByIdAsync(userRole.RoleName);

            if (role == null)
            {
                try
                {
                    IdentityRole newrole = new IdentityRole { Name = userRole.RoleName };
                    var result = await roleManager.CreateAsync(newrole);
                    if (result.Succeeded)
                    {
                        TempData["successMessage"] = $"add role: '{userRole.RoleName}' successfully.";
                        return RedirectToAction("Index");
                    }
                    else
                        TempData["Message"] = result.Errors.FirstOrDefault().Description;
                }
                catch (Exception e)
                {
                    TempData["Message"] = e.GetBaseException().Message;
                }
            }
            else
            {
                TempData["Message"] = $"Role name: '{userRole.RoleName}' already exists";
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError("", $"Role - {id} wasn't found");
                return NotFound();
            }
            ViewBag.roleName = role.Name;
            ViewBag.roleID = role.Id;

            var userList = new List<IdentityUser>();
            foreach (var user in userManager.Users)
            {
                var identityUser = new IdentityUser
                {
                    UserName = user.UserName,
                    Email = user.Email
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userList.Add(identityUser);
                }
            }

            if (userList.Any())
            {
                userList = userList.OrderBy(m => m.UserName).ToList();
                return View(userList);

            }
            else
            {
                try
                {
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        TempData["successMessage"] = $"{role.Name} was successfully deleted";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["Message"] = TempData["Message"] + error.Description + "\n";
                        }
                    }
                }
                catch (Exception e)
                {
                    TempData["Message"] = $"Exception occured: {e.GetBaseException().Message}";
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            ViewBag.roleName = role.Name;
            ViewBag.roleID = role.Id;
            //delete table aspnetuserole
            var userList = await userManager.GetUsersInRoleAsync(role.Name);

            IdentityResult removeResult;
            foreach (var user in userList)
            {
                try
                {
                    removeResult = await userManager.RemoveFromRoleAsync(user, role.Name);
                    if (removeResult.Succeeded)
                    {
                        TempData["successMessage"] = $"{role.Name} was successfully deleted" + "\n";
                    }
                    else
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            TempData["Message"] = TempData["Message"] + error.Description;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Exception occured: {ex.GetBaseException().Message}";
                }
            }

            try
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["successMessage"] = $"{role.Name} was successfully deleted";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["Message"] = TempData["Message"] + error.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Exception occured: {ex.GetBaseException().Message}";
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> UsersInRole(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            UserRole addedusers = new UserRole();
            if (role == null)
            {
                ModelState.AddModelError("", "This Role does not exists");
            }
            else
            {
                ViewBag.roleName = role.Name;
                ViewBag.roleId = role.Id;

                var notaddedUsers = new List<IdentityUser>();

                foreach (var user in userManager.Users)
                {
                    IdentityUser iUser = new IdentityUser { UserName = user.UserName, Email = user.Email };
                    if (await userManager.IsInRoleAsync(user, role.Name))
                    {
                        addedusers.UserInRole.Add(iUser);
                    }
                    else
                    {
                        notaddedUsers.Add(iUser);
                    }
                }

                if (notaddedUsers != null)
                    notaddedUsers = notaddedUsers.OrderBy(m => m.UserName).ToList();
                ViewBag.notAddedUsers = new SelectList(notaddedUsers, "UserName", "UserName");

                if (addedusers.UserInRole != null)
                    addedusers.UserInRole = addedusers.UserInRole.OrderBy(m => m.UserName).ToList();
            }
            return View(addedusers);
        }


        [HttpPost]
        public async Task<IActionResult> AddUserToRole(UserRole userRole)
        {
            var role = await roleManager.FindByIdAsync(userRole.RoleId); ;
            if (role == null)
            {
                TempData["Message"] = "Role does not exist in the file";
                return RedirectToAction("UsersInRole", new { id = userRole.RoleId });
            }

            var user = await userManager.FindByNameAsync(userRole.UserName);
            if (user == null)
            {
                TempData["Message"] = "User does not exist in the file";
                return RedirectToAction("UsersInRole", new { id = userRole.RoleId });
            }

            try
            {
                IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    TempData["successMessage"] = $"In Role '{role.Name}', add User ' {user.UserName}' successfully  ";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["Message"] += error.Description + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                TempData["Message"] = $"Exception: {e.GetBaseException().Message}";
            }

            return RedirectToAction("UsersInRole", new { id = userRole.RoleId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string RoleId, string UserName)
        {
            var role = await roleManager.FindByIdAsync(RoleId); ;
            if (role == null)
            {
                TempData["Message"] = "Role does not exist in the file";
                return RedirectToAction("UsersInRole", new { id = RoleId });
            }

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                TempData["Message"] = "User does not exist in the file";
                return RedirectToAction("UsersInRole", new { id = RoleId });
            }

            try
            {

                IdentityResult result = await userManager.RemoveFromRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    TempData["successMessage"] = $"In Role '{role.Name}', delete User '{user.UserName}' successfully";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["Message"] += error.Description + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                TempData["Message"] = $"Exception: {e.GetBaseException().Message}";
            }

            return RedirectToAction("UsersInRole", new { id = RoleId });
        }

    }
}
