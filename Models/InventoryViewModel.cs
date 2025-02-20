namespace StockMaster.Models
{
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; } // ✅ Store user's email
    }
}
