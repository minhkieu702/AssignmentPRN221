using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using SignalROrder.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SignalROrder.Controllers
{
    public class OrderController : Controller
    {
        private readonly Service _service;
        private readonly IHubContext<SignalrServer> _signalRHub;

        public OrderController(IHubContext<SignalrServer> signanRHub)
        {
            _service ??= new();
            _signalRHub = signanRHub;
            _service.ChangeSource("Json");
        }

        // GET: OrderController
        public ActionResult Index()
        {
            return View(_service.GetAllOrders().ToList());
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            var res = _service.GetAllOrders();
            return Ok(res);
        }
        // GET: OrderController/Details/5
        public ActionResult Details(int id)
        {
            Order order = _service.GetById(id);
            return View(new OrderWithCustomer
            {
                Order = order,
                Customers = _service.GetAllCustomers()
            });
        }

        // GET: OrderController/Create
        public ActionResult Create()
        {
            OrderWithCustomer orderWithCustomer = new OrderWithCustomer
            {
                Order = new(),
                Customers = _service.GetAllCustomers()
            };
            return View(orderWithCustomer);
        }

        // POST: OrderController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OrderWithCustomer model)
        {
            try
            {
                model.Customers = _service.GetAllCustomers();
                if (string.IsNullOrEmpty(model.Order.OrderNotes))
                {
                    model.Order.OrderNotes = string.Empty;
                }
                _service.Insert(model.Order);
                if (!_service.SaveChange("Json"))
                {
                    return View(model);
                }
                await _signalRHub.Clients.All.SendAsync("LoadOrders");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }

        // GET: OrderController/Edit/5
        public ActionResult Edit(int id)
        {
            return View(new OrderWithCustomer
            {
                Customers = _service.GetAllCustomers(),
                Order = _service.GetById(id)
            });
        }

        // POST: OrderController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, OrderWithCustomer model)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                model.Customers = _service.GetAllCustomers();
                _service.Update(model.Order);
                if (!_service.SaveChange("Json"))
                {
                    return View(model);
                }
                await _signalRHub.Clients.All.SendAsync("LoadOrders");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }

        // GET: OrderController/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = _service.GetById(id.Value);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: OrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            try
            {
                _service.Delete(id);
                _service.SaveChange("Json");
                await _signalRHub.Clients.All.SendAsync("LoadOrders");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
