using Ecommerce_Markets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Markets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {

        private readonly dbMarketsContext _context;

        public SearchController(dbMarketsContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                ls = _context.Products
                    .AsNoTracking()
                    .Include(x => x.Cat)
                    .OrderByDescending(x => x.ProductId)
                    .ToList();
                return  PartialView("ListProductsSearchPartial", ls);
            }

            ls = _context.Products.AsNoTracking()
                .Include(a => a.Cat)
                .Where(x => x.ProductName.Contains(keyword))
                .OrderByDescending(x => x.ProductName)
                .Take(10)
                .ToList();
            return PartialView("ListProductsSearchPartial", ls);

        }
    }
}
