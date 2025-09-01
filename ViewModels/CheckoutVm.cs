using System.ComponentModel.DataAnnotations;

namespace OnlineBookstore.ViewModels
{
    // Represents the checkout form data entered by a customer
    public class CheckoutVm
    {
        [Required]
        [Display(Name = "Pay with")]
        public string PaymentMethod { get; set; } = "Cash"; 

        [Required, StringLength(80)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(12)]
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required, StringLength(24)]
        public string Phone { get; set; } = string.Empty;
    }
}
