using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockMaster.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Links to IdentityUser

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Department { get; set; } // Optional field

        public bool IsActive { get; set; } = true; // Admin status
    }
}
