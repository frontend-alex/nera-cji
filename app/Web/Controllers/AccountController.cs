namespace nera_cji.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("app/v1/account")]
public class AccountController : Controller {
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger) {
        _logger = logger;
    }

    [HttpGet]
    [HttpGet("index")]
    public IActionResult Index() {
        return View();
    }
}

