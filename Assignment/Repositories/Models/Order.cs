using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data.Models
{
    public partial class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string Type { get; set; } = null!;

        public double TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public string? OrderNotes { get; set; }
        public bool? Delete { get; set; } = false;
        public virtual Customer Customer { get; set; } = null!;
        public override string ToString()
        {
            return OrderId + CustomerId + Type + TotalAmount + OrderDate + OrderNotes;
        }
    }
}
