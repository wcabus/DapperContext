using System.Collections.Generic;
using Dapper;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository.Sql
{
    public class CategoryRepository : BaseRepository, ICategoryRepository
    {
        public IEnumerable<Category> GetCategories()
        {
            return CreateContext().Query<Category>("SELECT * FROM Categories");
        }
    }
}