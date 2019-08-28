using EventFeed.Producer.Clicks;
using EventFeed.Producer.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventFeed.Producer.Controllers
{
    public class HomeController: Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            int clickCount = _storage.GetClickCount();
            return View(new HomeModel(clickCount));
        }

        [HttpPost("click")]
        public IActionResult Click()
        {
            _clickService.RegisterClick();
            return RedirectToAction("Index");
        }

        public HomeController(
            IClickService clickService,
            IClickStorage storage
        )
        {
            _clickService = clickService;
            _storage = storage;
        }

        private readonly IClickService _clickService;
        private readonly IClickStorage _storage;
    }
}
