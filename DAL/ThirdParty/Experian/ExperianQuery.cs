using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobDAL.ThirdParty.Experian
{
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Container of 'experian' related queries
    /// </summary>
    public class ExperianQuery : QueryBase {
        /// <summary>
        /// name of the stored procedure
        /// </summary>
        private static readonly string GetExperianConsumerScore = "GetExperianConsumerScore";
        /// <summary>
        /// the parameter of 'GetExperianConsumerScore' stored procedure
        /// </summary>
        private static readonly string CustomerId = "@CustomerId";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperianQuery"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public ExperianQuery(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Gets the experian score.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns>experian score</returns>
        internal int GetExperianScore(int customerId) {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(GetExperianConsumerScore, sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(CustomerId, SqlDbType.Int));
                    sqlCommand.Parameters[CustomerId].Value = customerId;

                    object score = sqlCommand.ExecuteScalar();
                    if (score == null || score is DBNull) {
                        return 0;
                    }

                    return Convert.ToInt32(score);
                }
            }
        }
    }
}
