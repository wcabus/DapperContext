using System.Collections;
using System.Collections.Generic;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetCustomerOrders(Customer customer);
        IEnumerable<OrderLine> GetOrderDetail(Order order);
        
    }
}