using Ecommerce_Markets.Models;

namespace Ecommerce_Markets.ModelViews
{
    public class ProductHomeVM
    {
        public Category category { get; set; }
        public List<Product> lsProducts { get; set; }
    }
}
