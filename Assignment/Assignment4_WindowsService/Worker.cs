using System.Text.Json;

namespace Assignment4_WindowsService
{
    public partial class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string Type { get; set; } = null!;

        public double TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public string? OrderNotes { get; set; }
        public bool? Delete { get; set; }
    }
    public class Worker : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Worker> _logger;
        private const int ThreadDelay = 5000;
        private readonly string FilePath = @"D:\now_semester\PRN221\AssignmentPRN221\Logging.txt";
        private readonly string jsonFilePath = "D:\\now_semester\\PRN221\\AssignmentPRN221\\Assignment\\Assignment4_WindowsService\\Orders.json";
        public Worker(ILogger<Worker> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
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
            var orders = LoadOrdersFromJson();
            ordersDeleteList = orders.Where(b => b.Delete == true).ToList();
            ordersExistList = orders.Where(b => b.Delete == false).ToList();
            orders.Where(b => b.Delete.Value).ToList().ForEach(o => orders.Remove(o));
            SaveOrdersToJson(orders);
        }

        private List<Order> LoadOrdersFromJson()
        {
            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<List<Order>>(jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool SaveOrdersToJson(List<Order> orders)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(jsonFilePath, jsonString);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
