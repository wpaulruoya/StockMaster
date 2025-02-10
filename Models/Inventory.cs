using Microsoft.AspNetCore.Identity;

namespace StockMaster.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Foreign Key
        public string UserId { get; set; }

        // Navigation Property
        public IdentityUser User { get; set; } // ✅ Add this line
    }
}
