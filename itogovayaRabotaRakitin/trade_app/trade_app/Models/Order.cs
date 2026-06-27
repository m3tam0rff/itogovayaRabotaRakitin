using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderDate { get; set; } = null!;
        public string? DeliveryDate { get; set; }
        public string Status { get; set; } = null!;
        public string PickupCode { get; set; } = null!;
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = [];
    }
}
