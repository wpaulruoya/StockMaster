﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockMaster.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockMaster.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly SmartStockDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoryController(SmartStockDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var userInventory = await _context.Inventories
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            return View(userInventory);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Inventory inventory)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            if (ModelState.IsValid)
            {
                inventory.UserId = user.Id;
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(inventory);
        }
    }
}
