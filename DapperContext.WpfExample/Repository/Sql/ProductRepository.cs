using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository.Sql
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public IEnumerable<Product> GetProducts()
        {
            return CreateContext().Query<Product>("SELECT * FROM Products");
        }

        public IEnumerable<Product> GetProducts(string name)
        {
            name = name.Replace("*", "%"); //SQL Server uses % as a wildcard instead of the more commonly used *
            return CreateContext().Query<Product>("SELECT * FROM Products WHERE ProductName LIKE @Name", new {name});
        }

        public IEnumerable<Product> GetProductsByCategory(Category category)
        {
            return CreateContext().Query<Product>("SELECT * FROM Products WHERE CategoryID = @CategoryID", new { category.CategoryID });
        }

        public Product GetProduct(int productID)
        {
            return CreateContext().Query<Product>("SELECT * FROM Products WHERE ProductID = @ProductID", new {productID}).SingleOrDefault();
        }
    }
}