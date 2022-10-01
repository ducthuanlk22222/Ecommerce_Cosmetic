using System;
using System.Collections.Generic;
using Ecommerce_Markets.Models;

namespace Ecommerce_Markets.ModelViews
{
    public class XemDonHang
    {
        public Order DonHang { get; set; }
        public List<OrderDetail> ChiTietDonHang { get; set; }
    }
}
