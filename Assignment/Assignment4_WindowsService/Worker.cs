using Data.Models;
using Services;
using System.Net.Http;
using System.Text.Json;

namespace Assignment4_WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Worker> _logger;
        private readonly Service _service;
        private const int ThreadDelay = 5000;
        private readonly string FilePath = @"D:\now_semester\PRN221\AssignmentPRN221\Logging.txt";

        public Worker(ILogger<Worker> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
            _service ??= new Service();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        DeleteWithDeleteProp(out List<Order> ordersDeleteList, out List<Order> ordersExistList);
                        await WriteLog(ordersDeleteList, ordersExistList);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the worker.");
                }

                await Task.Delay(ThreadDelay, stoppingToken);
            }
        }

        private async Task WriteLog(List<Order> ordersDeleteList, List<Order> ordersExistList)
        {
            var ordersDeleteListString = ordersDeleteList.Count.ToString();
            var ordersExistListString = ordersExistList.Count.ToString();

            var timeText = "Worker running at: " + DateTimeOffset.Now;
            var text = $"{timeText}\nDelete Order List: {ordersDeleteListString}\nExisting Order List: {ordersExistListString}\n";

            _logger.LogInformation(text);

            await File.AppendAllTextAsync(FilePath, text);
        }

        public void DeleteWithDeleteProp(out List<Order> ordersDeleteList, out List<Order> ordersExistList)
        {
            _service.ChangeSource("Json");
            var orders = _service.GetAllOrders();
            ordersDeleteList = orders.Where(b => b.Delete == true).ToList();
            ordersExistList = orders.Where(b => b.Delete == false).ToList();
            orders.Where(b => b.Delete.Value).ToList().ForEach(o => _service.Delete(o.OrderId));
            _service.SaveChange("Json");
        }
    }
}
