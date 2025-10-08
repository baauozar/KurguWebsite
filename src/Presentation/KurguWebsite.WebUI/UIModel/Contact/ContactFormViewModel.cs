using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KurguWebsite.WebUI.UIModel.Contact
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        [Remote(action: "ValidateName", controller: "Validation", HttpMethod = "GET")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        [StringLength(300, ErrorMessage = "Email must not exceed 300 characters")]
        [Remote(action: "ValidateEmail", controller: "Validation", HttpMethod = "GET")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        [Remote(action: "ValidatePhone", controller: "Validation", HttpMethod = "GET")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject must not exceed 200 characters")]
        [Remote(action: "ValidateSubject", controller: "Validation", HttpMethod = "GET")]
        public string Subject { get; set; } = "";

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        [Remote(action: "ValidateMessage", controller: "Validation", HttpMethod = "GET")]
        public string Message { get; set; } = "";
    }
}
