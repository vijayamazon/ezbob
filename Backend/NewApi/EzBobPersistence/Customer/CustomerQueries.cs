namespace EzBobPersistence.Customer {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.ModelBinding;
    using EzBobCommon;
    using EzBobModels;
    using EzBobPersistence;
    using EzBobPersistence.Customer.Commands;
    using EzBobPersistence.Loan;
    using EzBobPersistence.ThirdParty.Experian;

    /// <summary>
    /// Container of customer related queries
    /// </summary>
    public class CustomerQueries : QueryBase, ICustomerQueries {

        private static readonly string roleName = "Web"; //TODO: find some other way to obtain role

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

        [Injected]
        public ILoanQueries LoanQueries { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public CustomerQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordQuestion">The password question.</param>
        /// <param name="passwordAnswer">The password answer.</param>
        /// <param name="remoteIp">The remote ip.</param>
        /// <returns></returns>
        public User CreateUser(string email, string password, int passwordQuestion, string passwordAnswer, string remoteIp) {
            var connection = GetOpenedSqlConnection2();
            User user;
            using (var sqlConnection = connection) {

                var createCustomerCommand = new CreateUser()
                    .SetEmailAndPassword(email, password)
                    .SetSecurityQuestionId(passwordQuestion)
                    .SetSecurityQuestionAnswer(passwordAnswer)
                    .SetRoleName(roleName) //TODO: find some other way to obtain role
                    .SetRemoteIp(remoteIp);

                using (SqlCommand sqlCommand = createCustomerCommand.Get(sqlConnection.SqlConnection())) {
                     SqlDataReader reader = sqlCommand.ExecuteReader();
                    user = CreateModel<User>(reader);
                    user.RoleName = roleName; //TODO: look again (may be it could be possible to remove those lines by getting them from DB)
                    user.EmailAddress = email; //TODO: look again

                }
            }

            return user;
        }

        /// <summary>
        /// Gets the customer details.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public CustomerDetails GetCustomerDetails(int customerId) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                using (SqlCommand sqlCommand = new SqlCommand(GetPersonalInfo, sqlConnection.SqlConnection())) {
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

        /// <summary>
        /// Gets user identifier by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public long? GetUserIdByUserName(string userName) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {

                GetUserIdByUserName command = new GetUserIdByUserName()
                    .SetUserName(userName.ToLowerInvariant());

                using (SqlCommand sqlCommand = command.Get(sqlConnection.SqlConnection())) {

                    object result = sqlCommand.ExecuteScalar();

                    try {
                        if (!(result is DBNull)) {
                            return Convert.ToInt64(result);
                        }

                        return -1;
                    } catch (FormatException ex) {
                        Log.Error("could not obtain user id by email address", ex);
                    } catch (InvalidCastException ex) {
                        Log.Error("could not obtain user id by email address", ex);
                    } catch (OverflowException ex) {
                        Log.Error("could not obtain user id by email address", ex);
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Creates or updates the customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns>
        /// int - customer id, empty if it was nothing to save  in db
        /// </returns>
        public Optional<int> UpsertCustomer(Customer customer) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
//                var id = new Tuple<string, object>("Name", customer.Name);
                var id = new KeyValuePair<string, object>("Id", customer.Id);
                var cmd = GetUpsertCommand(customer, sqlConnection.SqlConnection(), Tables.Customer, Enumerable.Repeat(id, 1), "Id");
                if (!cmd.HasValue) {
                    return null;
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }


        /// <summary>
        /// Gets the customer by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Customer GetCustomerById(int id) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetSelectAllByWhereCommand(sqlConnection, "Id", id, Tables.Customer)) {
                    var dataReader = sqlCommand.ExecuteReader();
                    Customer customer = CreateModel<Customer>(dataReader);
                    return customer;
                }
            }
        }

        /// <summary>
        /// Gets the customer by mail.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public Customer GetCustomerByEmail(string email) {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                using (var sqlCommand = GetSelectAllByWhereCommand(sqlConnection, "Name", email, Tables.Customer))
                {
                    var dataReader = sqlCommand.ExecuteReader();
                    Customer customer = CreateModel<Customer>(dataReader);
                    return customer;
                }
            }
        }

        /// <summary>
        /// Saves the customer address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>
        /// true - success, false - failure, null - was nothing to save in db
        /// </returns>
        public bool? SaveCustomerAddress(CustomerAddress address) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                var id = new KeyValuePair<string, object>("CustomerId", address.CustomerId);
                
                Optional<SqlCommand> cmd = GetUpsertCommand(address, sqlConnection.SqlConnection(), Tables.CustomerAddress, Enumerable.Repeat(id, 1));
                if (!cmd.HasValue) {
                    return null;
                }

                using (var sqlCommand = cmd.GetValue())
                {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Determines whether [customer exists by reference number] [the specified reference number].
        /// </summary>
        /// <param name="refNumber">The reference number.</param>
        /// <returns></returns>
        public bool? IsCustomerExistsByRefNumber(string refNumber) {
            using (var sqlConnection = GetOpenedSqlConnection2())
            {
                using (var sqlCommand = GetCountWhereCommand(sqlConnection.SqlConnection(), "RefNumber", refNumber, Tables.Customer)) {
                    var result = sqlCommand.ExecuteScalar();
                    try
                    {
                        if (!(result is DBNull))
                        {
                            return Convert.ToInt32(result) > 0;
                        }
                    }
                    catch (FormatException ex)
                    {
                        Log.Error("could not check customer existence by RefNumber", ex);
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Error("could not check customer existence by RefNumber", ex);
                    }
                    catch (OverflowException ex)
                    {
                        Log.Error("could not check customer existence by RefNumber", ex);
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified customer is vip.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        public bool? IsVip(string emailAddress) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = GetCountWhereCommand(sqlConnection.SqlConnection(), "Email", emailAddress, Tables.VipRequest)) {
                    var result = sqlCommand.ExecuteScalar();
                    try
                    {
                        if (!(result is DBNull))
                        {
                            return Convert.ToInt32(result) > 0;
                        }
                    }
                    catch (FormatException ex)
                    {
                        Log.Error("could not check whether is vip", ex);
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Error("could not check whether is vip", ex);
                    }
                    catch (OverflowException ex)
                    {
                        Log.Error("could not check whether is vip", ex);
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Saves the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        /// <returns></returns>
        public bool? SaveCustomerRequestedLoan(CustomerRequestedLoan requestedLoan) {
            return LoanQueries.SaveCustomerRequestedLoan(requestedLoan);
        }

        /// <summary>
        /// Saves the customer session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public bool? SaveCustomerSession(CustomerSession session) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(session, sqlConnection.SqlConnection(), Tables.CustomerSession);
                if (!cmd.HasValue) {
                    return null;
                }
                
                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the source reference history.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sourceRefList">The source reference list.</param>
        /// <param name="visitTimeList">The visit time list.</param>
        /// <returns>true - success, false - failure, null - was nothing to save  in db</returns>
        public bool? SaveSourceRefHistory(int userId, string sourceRefList, string visitTimeList) {

            if (string.IsNullOrEmpty(sourceRefList) || string.IsNullOrEmpty(visitTimeList)) {
                Log.Warn("got empty sourceRefList or visitTimeList");
                return false;
            }

            DataTable dataTable = this.PrepareSourceRefHistory(sourceRefList, visitTimeList);
            if (dataTable.Rows.Count > 0) {
                using (var sqlConnection = GetOpenedSqlConnection2()) {
                    using (var sqlCommand = new SqlCommand("SaveSourceRefHistory", sqlConnection.SqlConnection())) {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.AddWithValue("@UserId", userId);
                        sqlCommand.Parameters.AddWithValue("@Lst", dataTable)
                            .SqlDbType = SqlDbType.Structured;

                        return ExecuteNonQueryAndLog(sqlCommand);
                    }
                }
            }

            Log.Info(string.Format("No sourceref history to save for user {0}.", userId));
            return null;
        }

        /// <summary>
        /// Saves the campaign source reference.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="campaignSrcRef">The campaign source reference.</param>
        /// <returns></returns>
        public bool SaveCampaignSourceRef(int userId, CampaignSourceRef campaignSrcRef) {

            if (campaignSrcRef == null) {
                return false;
            }

            campaignSrcRef.RSource = campaignSrcRef.RSource ?? "Direct";
            campaignSrcRef.RDate = campaignSrcRef.RDate ?? DateTime.UtcNow;

            using (var sqlConnection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = new SqlCommand("SaveCampaignSourceRef", sqlConnection.SqlConnection())) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("CustomerID", userId);

                    var dataTable = ConvertToDataTable(campaignSrcRef, new string[] {"Id", "CustomerId"});
                    SqlParameter tableValueParameter = sqlCommand.Parameters.AddWithValue("@Tbl", dataTable);
                    tableValueParameter.SqlDbType = SqlDbType.Structured;

                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the customer phone.
        /// </summary>
        /// <param name="customerPhone">The customer phone.</param>
        /// <returns></returns>
        public bool SaveCustomerPhone(CustomerPhone customerPhone) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {

                var cmd = GetInsertCommand(customerPhone, sqlConnection.SqlConnection(), Tables.CustomerPhones);
                if (!cmd.HasValue) {
                    return false;
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }
      
        /// <summary>
        /// Prepares the source reference history.
        /// </summary>
        /// <param name="sourceRefList">The source reference list.</param>
        /// <param name="visitTimeList">The visit time list.</param>
        /// <returns></returns>
        private DataTable PrepareSourceRefHistory(string sourceRefList, string visitTimeList) {
            string[] sourceRefs = (HttpUtility.UrlDecode(sourceRefList) ?? string.Empty).Split(';');
            string[] visitTimes = (HttpUtility.UrlDecode(visitTimeList) ?? string.Empty).Split(';');

            //convert to sequence of 'SourceRefEntry'
            //visitTimes may contain less items than sourceRefs, 
            //so we concatenate 'infinite' sequence of empty strings to compensate, in case of missing items
            var seq = sourceRefs.Zip(visitTimes.Concat(Enumerable.Repeat(string.Empty, int.MaxValue)), (s, t) => {
                string srcRef = s.Trim();
                if (string.IsNullOrEmpty(srcRef)) {
                    return null;
                }

                DateTime? visitTime = null;
                if (!string.IsNullOrEmpty(t)) {
                    DateTime time;
                    if (DateTime.TryParseExact(t, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time)) {
                        visitTime = time;
                    }
                }


                return new SourceRefEntry {
                    SourceRef = srcRef,
                    VisitTime = visitTime
                };
            })
                .Where(o => o != null);

            return ConvertToDataTable(seq);
        }
    }
}
