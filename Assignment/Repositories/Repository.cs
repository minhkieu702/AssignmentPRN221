using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Repositories
{
    public class Repository

    {
        private static string jsonFilePath = "../../../../Repositories/Files/Orders.json";
        private static string customerJsonFilePath = "../../../../Repositories/Files/Customers.json";
        private static string xmlFilePath = "../../../../Repositories/Files/Orders.xml";
        private static List<Order> orders = new List<Order>();
        private static List<Customer> customers = new List<Customer>();
        private static void LoadOrdersFromJson()
        {
            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);

                orders = JsonSerializer.Deserialize<List<Order>>(jsonString);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public Customer GetCustomer(int id)
        {
            try
            {
                string jsonString = File.ReadAllText(customerJsonFilePath);

                customers = JsonSerializer.Deserialize<List<Customer>>(jsonString);

                return customers.FirstOrDefault(c => c.CustomerId == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void LoadOrdersFromXml()
        {
            try
            {
                orders = new List<Order>();
                using Stream s1 = File.OpenRead(xmlFilePath);
                var xs = new XmlSerializer(typeof(List<Order>));
                orders = (List<Order>)xs.Deserialize(s1);
                s1.Close();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file '{xmlFilePath}' was not found: {e.Message}");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"There was an error deserializing the XML data: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }
        public void ChangeSource(string f)
        {
            if (f == "Json")
            {
                LoadOrdersFromJson();
                return;
            }
            LoadOrdersFromXml();
        }
        public bool SaveChange(string f)
        {
            string oldString;
            string newString;
            if (f == "Json")
            {
                oldString = File.ReadAllText(jsonFilePath);
                SaveOrdersToJson();
                newString = File.ReadAllText(jsonFilePath);
                return oldString == newString ? false : true;
            }
            oldString = File.ReadAllText(xmlFilePath);
            SaveOrdersToXml();
            newString = File.ReadAllText(xmlFilePath);
            return oldString == newString ? false : true;
        }
        private static void GetIndex(int id, out int num)
        {
            num = 0;
            foreach (var o in orders)
            {
                if (o.OrderId == id) num = orders.IndexOf(o);
            }
        }
        private static void SaveOrdersToXml()
        {
            try
            {
                var xs = new XmlSerializer(typeof(List<Order>));
                using (Stream s1 = File.Create(xmlFilePath))
                {
                    xs.Serialize(s1, orders);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static bool SaveOrdersToJson()
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

        public List<Order> GetAll()
        {
            return orders;
        }

        public List<Order> GetBySearching(string searchString)
        {
            return orders.Where(o => o.OrderNotes.Trim().ToUpper().Contains(searchString.Trim().ToUpper())).ToList();
            //return (from o in orders
            //        where o.OrderNotes.Trim().ToUpper().Contains(searchString.Trim().ToUpper())
            //        select o
            //        ).ToList();
        }

        public Order GetById(int id) => orders.FirstOrDefault(o => o.OrderId == id);

        public void Update(Order order)
        {
            GetIndex(order.OrderId, out int num);
            order.Customer = GetCustomer(order.CustomerId);
            orders[num] = order;
        }

        public void Insert(Order order)
        {
            int o = orders.Max(o => o.OrderId);
            order.Customer = GetCustomer(order.CustomerId);
            order.OrderId = o + 1;
            orders.Add(order);
        }

        public void Delete(int id)
        {
            GetIndex(id, out int num);
            orders.RemoveAt(num);
        }
    }
}
