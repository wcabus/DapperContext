using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository.Sql
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public IEnumerable<Customer> GetCustomers()
        {
            return CreateContext().Query<Customer>("SELECT * FROM Customers");
        }

        public Customer GetCustomer(string customerID)
        {
            return CreateContext().Query<Customer>("SELECT * FROM Customers WHERE CustomerID = @CustomerID", new {customerID}).SingleOrDefault();
        }
    }
}