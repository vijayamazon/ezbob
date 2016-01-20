namespace EzBobPersistence.ThirdParty.Yodlee {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using EzBobCommon;
    using EzBobModels.MarketPlace;
    using EzBobModels.Yodlee;
    using EzBobPersistence.MarketPlace;

    internal class YodleeQueries : QueryBase, IYodleeQueries {
        public YodleeQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// The yodlee internal identifier
        /// </summary>
        // look at the table: "MP_MarketplaceType"
        private static readonly Guid YodleeInternalId = Guid.Parse("{107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF}");
        
        public IMarketPlaceQueries MarketPlace { get; set; }

        /// <summary>
        /// Determines whether the user already have the specified content service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="contentServiceId">The content service identifier.</param>
        /// <returns></returns>
        public bool IsUserAlreadyHaveContentService(int customerId, int contentServiceId)
        {
            IList<CustomerMarketPlace> customerMarketPlaces = MarketPlace.GetCustomerMarketPlaces(customerId, YodleeInternalId);
            var existingSite = customerMarketPlaces.Select(o => DeserializeSecurityInfo(o.SecurityData))
                .FirstOrDefault(s => s.CsId == contentServiceId);
            return existingSite != null;
        }

        /// <summary>
        /// Gets the user content services.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUserContentServicesIds(int customerId) {
            IList<CustomerMarketPlace> customerMarketPlaces = MarketPlace.GetCustomerMarketPlaces(customerId, YodleeInternalId);
            return customerMarketPlaces.Select(o => DeserializeSecurityInfo(o.SecurityData)
                .CsId);
        } 

        /// <summary>
        /// Gets the user account.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public YodleeUserAccount GetUserAccount(int customerId) {
            //TODO: how to make it one request (if no account, book it)
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetSelectFirstByWhereCommand(sqlConnection, "CustomerId", customerId, "YodleeAccounts"))
                {
                    var userAccount = CreateModel<YodleeUserAccount>(sqlCommand.ExecuteReader());
                    if (userAccount == null) {
                        userAccount = BookUserAccount(customerId);
                    }

                    return userAccount;
                }
            }
        }

        /// <summary>
        /// Books the user account.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public YodleeUserAccount BookUserAccount(int customerId) {
            using (var sqlConnection = GetOpenedSqlConnection()) {

                string commandText = "UPDATE YodleeAccounts SET CustomerId = @CustomerId OUTPUT INSERTED.* WHERE Id = (SELECT TOP 1 Id FROM YodleeAccounts WHERE CustomerId IS NULL)";

                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = commandText;
                sqlCommand.Parameters.AddWithValue("@CustomerId", customerId);

                using (SqlCommand command = sqlCommand) {
                    SqlDataReader reader = command.ExecuteReader();
                    return CreateModel<YodleeUserAccount>(reader);
                }
            }
        }

        /// <summary>
        /// This function exists to bridge between new yodlee api and old yodlee api.<br/>
        /// 'ContentServiceId' is the same in both apis.<br/> 
        /// But new api contains a new element: 'Site' - which contains 'content services'
        /// <br/>
        /// Gets the site identifier from content service identifier.
        /// </summary>
        /// <param name="contentServiceId">The content service identifier.</param>
        /// <returns></returns>
        public Optional<int> GetSiteIdFromContentServiceId(int contentServiceId) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetSelectFirstByWhereCommand(sqlConnection, "ContentServiceId", contentServiceId, "YodleeSites")) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the user account.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public Optional<bool> UpsertContentServiceAccount(YodleeOrderItem item) {
            using (var slqConnection = GetOpenedSqlConnection()) {
                var where = Enumerable.Repeat(new KeyValuePair<string, object>("bankAccountId", item.bankAccountId), 1);
                var optionalCmd = GetUpsertCommand(item, slqConnection, "MP_YodleeOrderItem", where);
                if (optionalCmd.HasValue) {
                    Log.Info("was nothing to save in user account");
                    return null;
                }

                using (var sqlCommand = optionalCmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the transactions.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <returns></returns>
        public IList<bool?> UpsertTransactions(IEnumerable<YodleeOrderItemTransaction> transactions) {
            List<bool?> results = new List<bool?>();
            using (var slqConnection = GetOpenedSqlConnection())
            {
                foreach (var transaction in transactions) {
                    var where = Enumerable.Repeat(new KeyValuePair<string, object>("bankTransactionId", transaction.bankTransactionId), 1);
                    var optionalCmd = GetUpsertCommand(transaction, slqConnection, "MP_YodleeOrderItemBankTransaction", where);
                    if (optionalCmd.HasValue) {
                        Log.Info("was nothing to save in user account");
                        results.Add(null);
                        continue;
                    }

                    using (var sqlCommand = optionalCmd.GetValue()) {
                        results.Add(ExecuteNonQueryAndLog(sqlCommand));
                    }
                }
            }

            return results;
        }


        /// <summary>
        /// De-serializes the security information.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        /// <returns></returns>
        private YodleeSecurityInfo DeserializeSecurityInfo(byte[] blob)
        {
            using (var stream = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(YodleeSecurityInfo));
                return xmlSerializer.Deserialize(stream) as YodleeSecurityInfo;
            }
        }
    }
}
