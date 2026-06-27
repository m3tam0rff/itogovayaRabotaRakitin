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
        public virtual Manufacturer Manufacturer { get; set; } = null!;
    }
}
