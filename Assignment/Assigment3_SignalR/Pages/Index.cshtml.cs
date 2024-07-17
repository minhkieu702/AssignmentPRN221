using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assigment3_SignalR.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Service _service;
        private readonly IHubContext<SignalRServer> _hubContext;

        public List<Order> Orders { get; set; }

        public IndexModel(IHubContext<SignalRServer> hubContext)
        {
            _service ??= new();
            _hubContext = hubContext;
            _service.ChangeSource("Json");
        }

        public void OnGet()
        {
            Orders = _service.GetAllOrders();
        }

        public async Task<IActionResult> OnPost(int orderId)
        {
            var order = _service.GetById(orderId);
            if (order != null)
            {
                _service.Delete(orderId);
                _service.SaveChange("Json");
                await _hubContext.Clients.All.SendAsync("OrderDeleted", order);
            }
            return RedirectToPage();
        }
    }
}
