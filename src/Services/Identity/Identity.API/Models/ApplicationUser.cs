using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required] public string CardNumber { get; set; }
        [Required] public string CardHolderName { get; set; }

        [Required]
        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        public string Expiration { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV should match a valid 3 or 4 digit value")]
        public string CVV { get; set; }

        [Required] public string Street { get; set; }
        [Required] public string City { get; set; }
        [Required] public string Country { get; set; }
        [Required] public string ZipCode { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string LastName { get; set; }
    }
}