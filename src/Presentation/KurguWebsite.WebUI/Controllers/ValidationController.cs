using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace KurguWebsite.WebUI.Controllers
{
    [Route("validation")]
    public class ValidationController : Controller
    {
        [AcceptVerbs("GET")]
        [Route("validateName")]
        public IActionResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json("Name is required");
            if (name.Length > 100)
                return Json("Name must not exceed 100 characters");
            if (!Regex.IsMatch(name, @"^[a-zA-Z\s]+$"))
                return Json("Name can only contain letters and spaces");
            return Json(true);
        }

        [AcceptVerbs("GET")]
        [Route("validateEmail")]
        public IActionResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json("Email is required");
            if (email.Length > 300)
                return Json("Email must not exceed 300 characters");
            // Lightweight email check; DA already does EmailAddress on client.
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return Json("Invalid email format");

            // (Optional) check for duplicates using your app service:
            // if (await _userSvc.EmailExistsAsync(email)) return Json("This email is already in use");

            return Json(true);
        }

        [AcceptVerbs("GET")]
        [Route("validatePhone")]
        public IActionResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Json("Phone number is required");
            if (phone.Length > 20)
                return Json("Phone number must not exceed 20 characters");
            if (!Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]+$"))
                return Json("Invalid phone number format");
            return Json(true);
        }

        [AcceptVerbs("GET")]
        [Route("validateSubject")]
        public IActionResult ValidateSubject(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return Json("Subject is required");
            if (subject.Length > 200)
                return Json("Subject must not exceed 200 characters");
            return Json(true);
        }

        [AcceptVerbs("GET")]
        [Route("validateMessage")]
        public IActionResult ValidateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json("Message is required");
            if (message.Length < 10)
                return Json("Message must be at least 10 characters");
            if (message.Length > 2000)
                return Json("Message must not exceed 2000 characters");
            return Json(true);
        }
    }
}
