using Microsoft.AspNetCore.Mvc;

namespace nera_cji.Controllers
{
<<<<<<< Updated upstream:nera-cji/Controllers/EventsController.cs
=======
    [Route("app/v1/events")]
>>>>>>> Stashed changes:app/Web/Controllers/EventsController.cs
    public class EventsController : Controller
    {
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
<<<<<<< Updated upstream:nera-cji/Controllers/EventsController.cs

=======
>>>>>>> Stashed changes:app/Web/Controllers/EventsController.cs
