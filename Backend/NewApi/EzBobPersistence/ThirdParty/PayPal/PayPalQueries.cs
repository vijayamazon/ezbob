namespace EzBobPersistence.ThirdParty.PayPal {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using EzBobCommon;
    using EzBobModels.PayPal;

    public class PayPalQueries : QueryBase, IPayPalQueries {
        public PayPalQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Gets the last transaction date.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <returns></returns>
        public Optional<DateTime> GetLastTransactionDate(int marketPlaceId) {
            using (var connection = GetOpenedSqlConnection2()) {
                string query = "SELECT Max(Created) FROM MP_PayPalTransaction WHERE CustomerMarketPlaceId = @MarketPlaceId";
                using (var sqlCommand = GetEmptyCommand(connection.SqlConnection())) {
                    sqlCommand.CommandText = query;
                    sqlCommand.Parameters.AddWithValue("@MarketPlaceId", marketPlaceId);
                    return ExecuteScalarAndLog<DateTime>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the personal information.
        /// </summary>
        /// <param name="personalInfo">The personal information.</param>
        /// <returns></returns>
        public Optional<int> SavePersonalInfo(PayPalUserPersonalInfo personalInfo) {
            using (var connection = GetOpenedSqlConnection2()) {

                var cmd = GetInsertCommand(connection, connection.SqlConnection(), "MP_PayPalPersonalInfo", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return -1;
                }
                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public Optional<int> SaveTransaction(PayPalTransaction transaction) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(transaction, connection.SqlConnection(), "MP_PayPalTransaction", "Id");
                if (!cmd.HasValue) {
                    return Optional<int>.Empty();
                }

                using (var sqlCommand = cmd.Value) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        } 

        /// <summary>
        /// Saves the transaction items.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <returns></returns>
        public Optional<bool> SaveTransactionItems(IEnumerable<PayPalTransactionItem> transactions) {
            using (var connection = GetOpenedSqlConnection2()) {
                foreach (var batch in transactions.Batch(800)) {
                    var cmd = GetMultiInsertCommand(batch, connection.SqlConnection(), "MP_PayPalTransactionItem2", SkipColumns("Id"));
                    if (!cmd.HasValue) {
                        return Optional<bool>.Empty();
                    }
                    using (var command = (SqlCommand)cmd) {
                        if (!ExecuteNonQueryAndLog(command)) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
