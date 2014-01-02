using System.Collections;
using System.Collections.Generic;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetProducts();
        IEnumerable<Product> GetProducts(string name);
        IEnumerable<Product> GetProductsByCategory(Category category);
        Product GetProduct(int productID);
    }
}