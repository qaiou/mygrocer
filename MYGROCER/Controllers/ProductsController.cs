using Microsoft.AspNetCore.Mvc;

namespace MYGROCER.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
