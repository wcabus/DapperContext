using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Services
{
    public interface IOrderService
    {
        Order CreateOrder(Customer customer);
        OrderLine CreateOrderDetail(Order order, Product product);
        OrderLine CreateOrderDetail(Order order, Product product, decimal price, short quantity, float discount);

        void SaveOrder(Order order);
    }
}