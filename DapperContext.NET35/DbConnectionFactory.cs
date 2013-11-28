using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Dapper
{
    public class DbConnectionFactory
    {
        private readonly DbProviderFactory _provider;
        private readonly string _connectionString;
        private readonly string _name;

        public DbConnectionFactory(string connectionStringName)
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");

            var conStr = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (conStr == null)
                throw new ConfigurationErrorsException(
                    string.Format("Failed to find connection string named '{0}' in app.config or web.config.", connectionStringName));

            _name = conStr.ProviderName;
            _provider = DbProviderFactories.GetFactory(conStr.ProviderName);
            _connectionString = conStr.ConnectionString;
        }

        public IDbConnection Create()
        {
            var connection = _provider.CreateConnection();
            if (connection == null)
                throw new ConfigurationErrorsException(
                    string.Format(
                        "Failed to create a connection using the connection string named '{0}' in app.config or web.config.",
                        _name));

            connection.ConnectionString = _connectionString;
            return connection;
        }
    }
}