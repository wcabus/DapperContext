using System.Configuration;
using Dapper;

namespace DapperContext.WpfExample.Repository.Sql
{
    public abstract class BaseRepository
    {
        protected DbContext CreateContext()
        {
            return new DbContext(ConfigurationManager.AppSettings["Database"]);
        }
    }
}