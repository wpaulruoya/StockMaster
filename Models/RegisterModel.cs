using System.ComponentModel.DataAnnotations;

namespace StockMaster.Models
{
    public class RegisterModel
    {
        public string FullName { get; set; }  // Optional
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
