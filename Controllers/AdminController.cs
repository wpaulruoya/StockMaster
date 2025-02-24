﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StockMaster.Models;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmartStockDbContext _context; // ✅ Inject SmartStockDbContext

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SmartStockDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; // ✅ Assign injected context
        }

        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }

        public async Task<IActionResult> Dashboard()
        {
            PreventPageCaching();
            var recentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync();
            ViewBag.RecentUsers = recentUsers;
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = new List<UserWithRoleViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains("SuperAdmin")) // Exclude SuperAdmin users
                    {
                        userList.Add(new UserWithRoleViewModel
                        {
                            Id = user.Id,
                            Email = user.Email,
                            Role = roles.FirstOrDefault() ?? "User"
                        });
                    }
                }

                ViewBag.Users = userList; // ✅ Pass user list with roles to the view
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while fetching users.";
                return View("Error");
            }
        }

        public IActionResult ManageInventory()
        {
            try
            {
                var inventoryData = (from inv in _context.Inventories
                                     join usr in _context.Users on inv.UserId equals usr.Id into userGroup
                                     from user in userGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         inv.Id,
                                         inv.Name,
                                         inv.Quantity,
                                         inv.Price,
                                         inv.UserId,
                                         UserEmail = user != null ? user.Email : "Unknown" // ✅ Get User Email
                                     }).ToList();

                ViewBag.Users = _context.Users.Select(u => new { u.Id, u.Email }).ToList();
                ViewBag.Inventory = inventoryData;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching inventory: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while fetching inventory.";
                return View("Error");
            }
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears session data
            return RedirectToAction("Index", "Home"); // Redirects to the login page
        }
    }

    public class UserWithRoleViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
