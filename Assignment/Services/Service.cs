﻿using Data.Models;
using Repositories;

namespace Services
{
    public class Service
    {
        Repository _repo = new();
        public List<Order> GetAllOrders() => _repo.GetAll();
        public List<Order> Search(string searchStr) => _repo.GetBySearching(searchStr);
        public void Update(Order order) => _repo.Update(order);
        public void Insert(Order order) => _repo.Insert(order);
        public Order GetById(int id) => _repo.GetById(id);
        public void Delete(int id) => _repo.Delete(id);
        public bool SaveChange(string f) => _repo.SaveChange(f);
        public void ChangeSource(string f) => _repo.ChangeSource(f);
        public Customer GetCustomer(int id) => _repo.GetCustomer(id);
        public List<Customer> GetAllCustomers() => _repo.GetCustomers();
        public string LoadTextFromFile(string f) => _repo.LoadTextFile(f);
        public bool SaveFile(string type, string content) => _repo.SaveFile(type, content);
    }
}
