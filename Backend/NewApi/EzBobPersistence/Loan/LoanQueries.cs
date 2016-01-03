namespace EzBobPersistence.Loan {
    using System.Data;
    using System.Data.SqlClient;
    using EzBobModels;
    using EzBobModels.Loan;
    using EzBobModels.Loan.Enums;
    using EzBobPersistence;

    /// <summary>
    /// Container of loan related queries
    /// </summary>
    public class LoanQueries : QueryBase, ILoanQueries {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoanQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public LoanQueries(string connectionString)
            : base(connectionString) {}

        public LoanSource GetLoanSource(LoanKind loanKind) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand("GetLoanSource", sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter("@LoanSourceID", SqlDbType.Int));
                    sqlCommand.Parameters["@LoanSourceID"].Value = (int)loanKind;

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    LoanSource customerDetails = CreateModel<LoanSource>(reader);

                    return customerDetails;
                }
            }
        }

        /// <summary>
        /// Saves the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        /// <returns>true - success, false - failure, null - was nothing to save in db</returns>
        public bool? SaveCustomerRequestedLoan(CustomerRequestedLoan requestedLoan) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                var cmd = GetInsertCommand(requestedLoan, sqlConnection, Tables.CustomerRequestedLoan);
                if (!cmd.HasValue) {
                    return null;
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }
    }
}
