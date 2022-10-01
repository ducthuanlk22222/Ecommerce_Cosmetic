using AspNetCoreHero.ToastNotification.Abstractions;
using Ecommerce_Markets.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Markets.Controllers
{
    public class LocationController : Controller
    {
        private readonly dbMarketsContext _context;
        public INotyfService _notifyService { get; }
        public LocationController(dbMarketsContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }
        public IActionResult Index()
        {
            return View();
        }
        //get location
        public ActionResult DistrictList(int LocationId)
        {
            var districts = _context.Locations.OrderBy(x => x.LocationId)
                .Where(x => x.Parent == LocationId && x.Levels == 2)
                .OrderBy(x => x.Name)
                .ToList();
            return Json(districts);
        }
        public ActionResult WardList(int LocationId)
        {
            var wards = _context.Locations.OrderBy(x => x.LocationId)
                .Where(x => x.Parent == LocationId && x.Levels == 3)
                .OrderBy(x => x.Name)
                .ToList();
            return Json(wards);
        }
    }
}
