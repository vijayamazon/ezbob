using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobDAL
{
    using System.Data;
    using System.Data.SqlClient;
    using EzBobCommon;
    using EzBobDAL.ThirdParty.Experian;
    using Models;

    /// <summary>
    /// Container of customer related queries
    /// </summary>
    public class CustomerQueries : QueryBase {

        /// <summary>
        /// name of stored procedure
        /// </summary>
        private static readonly string GetPersonalInfo = "GetPersonalInfo";
        /// <summary>
        /// The customer identifier, parameter of 'GetPersonalInfo' stored procedure
        /// </summary>
        private static readonly string CustomerId = "@CustomerId";

        /// <summary>
        /// Injected by IOC container
        /// </summary>
        /// <value>
        /// The experian query.
        /// </value>
        [Injected]
        public ExperianQuery ExperianQuery { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public CustomerQueries(string connectionString)
            : base(connectionString) { }

        /// <summary>
        /// Gets the customer details.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public CustomerDetails GetCustomerDetails(int customerId)
        {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand(GetPersonalInfo, sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(CustomerId, SqlDbType.Int));
                    sqlCommand.Parameters[CustomerId].Value = customerId;

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    CustomerDetails customerDetails = CreateModel<CustomerDetails>(reader);
                    if (customerDetails.Id == 0) {
                        customerDetails.Id = customerId;
                    }
                    return customerDetails;
                }
            }
        }

    }
}
