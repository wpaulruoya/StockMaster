using System.ComponentModel.DataAnnotations;

namespace StockMaster.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string UserId { get; set; } // Tie inventory to a specific user
    }

}
