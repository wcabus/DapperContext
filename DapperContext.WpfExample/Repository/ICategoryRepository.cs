using System.Collections;
using System.Collections.Generic;
using DapperContext.WpfExample.Model;

namespace DapperContext.WpfExample.Repository
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetCategories();
    }
}