using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace StockMaster.Models
{
    public class AdminViewModel
    {
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public int TotalUsers { get; set; }
        public int TotalInventoryItems { get; set; }
        public int PendingOrders { get; set; } // Placeholder (Extendable)
    }
}
