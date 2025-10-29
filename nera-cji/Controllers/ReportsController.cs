using Microsoft.AspNetCore.Mvc;

namespace nera_cji.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Generate()
        {
            return View();
        }
    }
}

