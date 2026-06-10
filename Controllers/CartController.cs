using Microsoft.AspNetCore.Mvc;

namespace MYGROCER.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
