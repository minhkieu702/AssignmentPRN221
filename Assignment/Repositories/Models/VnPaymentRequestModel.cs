using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class VnPaymentRequestModel
    {
        public string OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string FullName { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
