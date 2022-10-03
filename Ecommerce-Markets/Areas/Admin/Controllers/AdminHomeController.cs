using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Markets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin.html", Name = "AdminIndex")]
    public class AdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
