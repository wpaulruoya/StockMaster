using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace StockMaster.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            // Fetch users from the database
            return View();
        }

        public IActionResult ManageRoles()
        {
            // Fetch roles from the database
            return View();
        }
    }
}
