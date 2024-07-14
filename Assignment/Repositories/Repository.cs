using Data.Models;
using Microsoft.Extensions.Configuration;
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
        private static string customerJsonFilePath;
        private static string jsonFilePath;
        private static string xmlFilePath;
        private static List<Order> orders = new List<Order>();
        private static List<Customer> customers = new List<Customer>();
        public Repository()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            customerJsonFilePath = configuration["CustomerJsonFilePath"];
            jsonFilePath = configuration["OrdersJsonFilePath"];
            xmlFilePath = configuration["OrdersXmlFilePath"];
        }
        private static void LoadOrdersFromJson()
        {
            try
            {
                orders = new();
                string jsonString = File.ReadAllText(jsonFilePath);
                orders = JsonSerializer.Deserialize<List<Order>>(jsonString);
                LoadCustomerFromFile();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Customer GetCustomer(int id)
        {
            return customers.FirstOrDefault(c => c.CustomerId == id);
        }
        private static void LoadCustomerFromFile()
        {
            try
            {
                string jsonString = File.ReadAllText(customerJsonFilePath);

                customers = JsonSerializer.Deserialize<List<Customer>>(jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Customer> GetCustomers()
        {
            return customers;
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
                LoadCustomerFromFile();
            }
            catch (Exception)
            {
                throw;
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
        public string LoadTextFile(string f)
        {
            string str = File.ReadAllText(xmlFilePath);
            if (f == "Json")
            {
                str = File.ReadAllText(jsonFilePath);
            }
            return str;
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
            }
            else
            {
                oldString = File.ReadAllText(xmlFilePath);
                SaveOrdersToXml();
                newString = File.ReadAllText(xmlFilePath);
            }
            return oldString != newString;
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
        
        public bool SaveFile(string type, string content)
        {
            try
            {
                string oldString;
                string newString;
                if (type == "Json")
                {
                    oldString = File.ReadAllText(jsonFilePath);

                    string jsonString = JsonSerializer.Serialize(
                        JsonSerializer.Deserialize<List<Order>>(content)
                        , new JsonSerializerOptions { WriteIndented = true });

                    File.WriteAllText(jsonFilePath, jsonString);

                    newString = File.ReadAllText(jsonFilePath);
                }
                else
                {
                    oldString = File.ReadAllText(xmlFilePath);

                    File.WriteAllText(xmlFilePath, content);

                    newString = File.ReadAllText(xmlFilePath);
                }

                return newString != oldString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private List<Order> DeserializeXmlToOrder(string xmlContent)
        {
            var serializer = new XmlSerializer(typeof(List<Order>));
            using (StringReader reader = new StringReader(xmlContent))
            {
                return (List<Order>)serializer.Deserialize(reader);
            }
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
