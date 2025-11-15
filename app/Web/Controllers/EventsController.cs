using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace nera_cji.Controllers
{
    [Authorize]
    [Route("app/v1/events")]
    public class EventsController : Controller
    {
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            return View();
        }
    }
}


