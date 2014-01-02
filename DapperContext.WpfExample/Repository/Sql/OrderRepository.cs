using System.Collections.Generic;
using Dapper;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository.Sql
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public IEnumerable<Order> GetCustomerOrders(Customer customer)
        {
            return CreateContext().Query<Order>("SELECT * FROM Orders WHERE CustomerID = @CustomerID", new {customer.CustomerID});
        }

        public IEnumerable<OrderLine> GetOrderDetail(Order order)
        {
            return CreateContext().Query<OrderLine>("SELECT * FROM [Order Details] WHERE OrderID = @OrderID",
                new {order.OrderID});
        }
    }
}