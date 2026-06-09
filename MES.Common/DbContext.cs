using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace MES.Common
{
    public static class DbContext
    {
        private static readonly string _connStr;

        static DbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            _connStr = config.GetConnectionString("MESConn")
                ?? throw new InvalidOperationException("MESConn not found in appsettings.json");
        }

        public static IDbConnection GetConnection()
            => new SqlConnection(_connStr);
    }
}
