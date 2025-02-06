using System.ComponentModel.DataAnnotations;

namespace StockMaster.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string ItemName { get; set; } // Use 'required' modifier (C# 11+)

        [Required]
        public int Quantity { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
