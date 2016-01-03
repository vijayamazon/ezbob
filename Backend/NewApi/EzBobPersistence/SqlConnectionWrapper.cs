using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobPersistence
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using Microsoft.Practices.EnterpriseLibrary.Data;

    public class SqlConnectionWrapper : IDisposable {
        private SqlConnection connection;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SqlConnectionWrapper(SqlConnection connection) {
            this.connection = connection;
        }

        public SqlConnection Connection
        {
            get { return this.connection; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
        
        }
    }

    public static class DatabaseConnectionWrapperExtensions
    {
        public static SqlConnection SqlConnection(this DatabaseConnectionWrapper wrapper) {
            return wrapper.Connection as SqlConnection;
        }

        public static SqlConnection SqlConnection(this SqlConnectionWrapper wrapper) {
            return wrapper.Connection;
        }
    }
}
