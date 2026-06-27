using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeApp.Models
{
    public class Product
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int ManufacturerId { get; set; }
        public int Discount { get; set; }
        public int InStock { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public virtual Manufacturer Manufacturer { get; set; } = null!;
        public decimal GetPriceWithDiscount()
        {
            if (Discount <= 0) return Price;
            return Price * (1 - Discount / 100m);
        }
        public decimal PriceWithDiscount => GetPriceWithDiscount();
    }
}
