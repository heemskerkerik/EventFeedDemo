using EventFeed.Consumer.Events;
using EventFeed.Consumer.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventFeed.Consumer.Controllers
{
    public class HomeController: Controller
    {
        private readonly ICachedClickStorage _storage;

        [HttpGet("")]
        public IActionResult Index()
        {
            var clickCount = _storage.GetClickCount();
            var model = new HomeModel { ClickCount = clickCount };

            return View(model);
        }

        public HomeController(ICachedClickStorage storage)
        {
            _storage = storage;
        }
    }
}
