using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Markets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public ActionResult ModalPopUp()
        {
            return View();
        }
    }
}
