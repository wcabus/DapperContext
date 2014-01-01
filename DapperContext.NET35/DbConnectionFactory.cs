using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Dapper
{
    /// <summary>
    /// A DbConnectionFactory allows you to create <see cref="IDbConnection"/> instances by configuring
    /// a connection in the connectionstrings section inside a app/web.config file.
    /// </summary>
    public class DbConnectionFactory
    {
        private readonly DbProviderFactory _provider;
        private readonly string _connectionString;
        private readonly string _name;

        /// <summary>
        /// Creates a new DbConnectionFactory instance.
        /// </summary>
        /// <param name="connectionStringName">A key of one of the connectionstring settings inside the connectionstrings section of an app/web.config file.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringName"/> is a null value.4</exception>
        /// <exception cref="ConfigurationErrorsException">Thrown if <paramref name="connectionStringName"/> is not found in any app/web.config file available to the application.</exception>
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

        /// <summary>
        /// Creates a new instance of <see cref="IDbConnection"/>.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">Thrown if the connectionstring entry in the app/web.config file is missing information, contains errors or is missing entirely.</exception>
        /// <returns></returns>
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