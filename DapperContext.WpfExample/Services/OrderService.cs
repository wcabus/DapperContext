using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using DapperContext.WpfExample.Model;
using DapperContext.WpfExample.Repository;
using DapperContext.WpfExample.Repository.Sql;

namespace DapperContext.WpfExample.Services
{
    public class OrderService : BaseRepository, IOrderService
    {
        private readonly ICustomerRepository _customerRepository;

        public OrderService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        private Customer GetDefaultCustomer()
        {
            return _customerRepository.GetCustomers().First();
        }

        public Order CreateOrder(Customer customer)
        {
            if (customer == null)
                customer = GetDefaultCustomer();

            return new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                Lines = new List<OrderLine>()
            };
        }

        public OrderLine CreateOrderDetail(Order order, Product product)
        {
            var line = new OrderLine
            {
                ProductID = product.ProductID,
                Quantity = 1,
                UnitPrice = product.UnitPrice.GetValueOrDefault(0)
            };

            order.Lines.Add(line);
            return line;
        }

        public OrderLine CreateOrderDetail(Order order, Product product, decimal price, short quantity, float discount)
        {
            var line = new OrderLine
            {
                ProductID = product.ProductID,
                Discount = discount,
                Quantity = quantity,
                UnitPrice = price
            };

            order.Lines.Add(line);
            return line;
        }

        public void SaveOrder(Order order)
        {
            using (var context = CreateContext())
            {
                using (var scope = context.CreateUnitOfWork())
                {
                    order.OrderID = context.Query<int>("" +
                        "INSERT INTO Orders (CustomerID, OrderDate) VALUES (@CustomerID, @OrderDate);" +
                        "SELECT CAST(SCOPE_IDENTITY() as int)",
                        new {order.CustomerID, OrderDate = order.OrderDate.Value}).Single();

                    foreach (var line in order.Lines)
                    {
                        line.OrderID = order.OrderID;
                        context.Execute("INSERT INTO [Order Details] (OrderID, ProductID, Discount, UnitPrice, Quantity) VALUES (@OrderID, @ProductID, @Discount, @UnitPrice, @Quantity)",
                        new { line.OrderID, line.ProductID, line.Discount, line.UnitPrice, line.Quantity });
                    }

                    scope.SaveChanges(); //And commit this unit of work
                }
            }
        }
    }
}