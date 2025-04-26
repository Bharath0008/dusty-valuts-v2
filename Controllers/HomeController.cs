using Microsoft.AspNetCore.Mvc;

namespace LinkedInAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}
