using Intex.Models;
using Microsoft.AspNetCore.Authorization;
namespace INTEX_AURORA_BRICKS.Controllers;

using Intex.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intex.Infastructure;
using SQLitePCL;
using System.Diagnostics;
using System.Drawing.Printing;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
public class UserAdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public UserAdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> UserAdmin()
    {
        var users = _userManager.Users.ToList();
        var userRolesViewModel = new List<UserRoles>();
        foreach (IdentityUser user in users)
        {
            var thisViewModel = new UserRoles();
            thisViewModel.UserId = user.Id;
            thisViewModel.Email = user.Email;
            thisViewModel.Roles = await GetUserRoles(user);
            userRolesViewModel.Add(thisViewModel);
        }
        ViewData["Roles"] = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(userRolesViewModel);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult ListUsers()
    {
        var users = _userManager.Users;
        return View(users);
    }
    private async Task<List<string>> GetUserRoles(IdentityUser user)
    {
        return new List<string>(await _userManager.GetRolesAsync((IdentityUser)user));
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UserAdmin(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();
        var selectedRoles = roles;
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded)
        {
            // Handle the error
            return View();
        }
        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            // Handle the error
            return View();
        }
        return RedirectToAction("UserAdmin");
    }





    [Authorize(Roles = "Admin")]
  
    

        // GET method to edit a user
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new UserRoles // Assuming UserRoles is a suitable ViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        // POST method to update a user
        [HttpPost]
        public async Task<IActionResult> EditUser(UserRoles model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email; // Update other fields as necessary
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            return RedirectToAction(nameof(UserAdmin)); // Redirect to the list of users/admin view
        
    }












}

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> EditUser(string id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return View("Error"); // Or redirect to a 'not found' page
        //    }

        //    var model = new EditUserViewModel
        //    {
        //        UserId = user.Id,
        //        UserName = user.UserName,
        //        Email = user.Email
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> EditUser(EditUserViewModel model)
        //{
        //    var user = await _userManager.FindByIdAsync(model.UserId);
        //    if (user == null)
        //    {
        //        return View("Error"); // Or redirect to a 'not found' page
        //    }

        //    user.Email = model.Email;
        //    user.UserName = model.UserName;

        //    var result = await _userManager.UpdateAsync(user);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("UserAdmin");
        //    }
        //    else
        //    {
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }
        //        return View(model);
        //    }
        //}
    

