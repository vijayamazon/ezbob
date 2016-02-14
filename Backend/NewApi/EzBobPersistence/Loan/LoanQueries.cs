namespace EzBobPersistence.Loan {
    using System.Data;
    using System.Data.SqlClient;
    using EzBobCommon;
    using EzBobModels.Customer;
    using EzBobModels.Loan;
    using EzBobModels.Loan.Enums;
    using EzBobPersistence;
    using EzBobPersistence.QueryGenerators;

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

        /// <summary>
        /// Gets the loan source.
        /// </summary>
        /// <param name="loanKind">Kind of the loan.</param>
        /// <returns></returns>
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
        /// Upserts the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        public Optional<int> UpsertCustomerRequestedLoan(CustomerRequestedLoan requestedLoan) {
            using (var connection = GetOpenedSqlConnection2()) {

                var cmd = GetUpsertGenerator(requestedLoan)
                    .WithConnection(connection.SqlConnection())
                    .WithSkipColumns(o => o.Id)
                    .WithMatchColumns(o => o.CustomerId)
                    .WithOutputColumns(o => o.Id)
                    .WithTableName(Tables.CustomerRequestedLoan)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the customer's requested loan.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public Optional<CustomerRequestedLoan> GetCustomerRequestedLoan(int customerId) {
            using (var connection = GetOpenedSqlConnection2()) {

                var cmd = new SelectWhereGenerator<CustomerRequestedLoan>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.CustomerRequestedLoan)
                    .WithSelect() //select *
                    .WithWhere(o => o.CustomerId, customerId)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return CreateModel<CustomerRequestedLoan>(sqlCommand.ExecuteReader());
                }
            }
        }
    }
}
