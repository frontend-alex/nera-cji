using Microsoft.AspNetCore.Mvc;
using nera_cji.ViewModels;

namespace nera_cji.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Home/Contact")]
        [HttpGet("contact")]
        public IActionResult Contact()
        {
            var model = new ContactFormViewModel();

            if (TempData.TryGetValue("ContactSuccess", out var successMessage))
            {
                ViewBag.Message = successMessage;
            }

            return View(model);
        }

        [HttpPost("Home/Contact")]
        [HttpPost("contact")]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // In a real implementation, send the inquiry to CRM or email.
                var fullName = $"{model.FirstName} {model.LastName}".Trim();
                _logger.LogInformation(
                    "Contact form submitted by {Name} ({Email}) for {Topic} from {Company} in {Country}",
                    string.IsNullOrWhiteSpace(fullName) ? "Unknown" : fullName,
                    model.Email,
                    model.Topic?.ToString() ?? "Unspecified",
                    model.Organization,
                    model.Country);

                TempData["ContactSuccess"] = "Thanks, you will hear back soon";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on contact form");
                ModelState.AddModelError(string.Empty, "Something went wrong while sending your message. Please try again.");
                return View(model);
            }

            return RedirectToAction(nameof(Contact));
        }
    }
}
