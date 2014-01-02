using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository.Sql
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        public IEnumerable<Employee> GetEmployees()
        {
            return CreateContext().Query<Employee>("SELECT * FROM Employees");
        }

        public Employee GetEmployee(int employeeID)
        {
            return CreateContext().Query<Employee>("SELECT * FROM Employees WHERE EmployeeID = @EmployeeID", new {employeeID}).SingleOrDefault();
        }
    }
}