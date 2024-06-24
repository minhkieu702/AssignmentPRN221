using Data.Models;

namespace SignalROrder.Models
{
    public class OrderWithCustomer
    {
        public Order Order { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
    }
}
