using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobPersistence.ThirdParty.Ebay {
    using System.Collections;
    using System.Data.SqlClient;
    using System.Runtime.InteropServices;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobModels.EBay;
    using EzBobPersistence.MarketPlace;
    using Microsoft.Practices.EnterpriseLibrary.Data;

    public class EbayQueries : QueryBase {
        /// <summary>
        /// The e-bay internal identifier
        /// </summary>
        // look at the table: "MP_MarketplaceType"
        private static readonly Guid EbayInternalId = Guid.Parse("{A7120CB7-4C93-459B-9901-0E95E7281B59}");

        public EbayQueries(string connectionString)
            : base(connectionString) {}


        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        public DateTime GetLastKnownEbayTransactionTime(int marketPlaceId) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the user accounts.
        /// </summary>
        /// <param name="ebayUserAccountData">The ebay user account data.</param>
        /// <param name="additionalAccounts">The additional accounts.</param>
        /// <returns></returns>
        public bool SaveUserAccounts(EbayUserAccountData ebayUserAccountData, IEnumerable<EbayAdditionalUserAccountData> additionalAccounts) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(ebayUserAccountData, sqlConnection.SqlConnection(), "MP_EbayUserAccountData", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return false;
                }

                int userAccountId;

                using (var sqlCommand = cmd.GetValue()) {
                    userAccountId = ExecuteScalarAndLog<int>(sqlCommand);
                    if (userAccountId < 1) {
                        return false;
                    }
                }

                return SaveAdditionalUserAccounts(sqlConnection.SqlConnection(), userAccountId, additionalAccounts);
            }
        }

        /// <summary>
        /// Saves the user data.
        /// </summary>
        /// <param name="ebayUserData">The ebay user data.</param>
        /// <param name="registrationAddress">The registration address.</param>
        /// <param name="sellerPaymentAddress">The seller payment address.</param>
        /// <returns></returns>
        public bool SaveUserData(EbayUserData ebayUserData, EbayUserAddressData registrationAddress, EbayUserAddressData sellerPaymentAddress) {

            if (registrationAddress != null) {
                Optional<int> registrationAddressId = SaveUserAddress(registrationAddress);
                if (!registrationAddressId.HasValue) {
                    return false;
                }

                ebayUserData.RegistrationAddressId = registrationAddressId.GetValue();
            }

            if (sellerPaymentAddress != null) {
                Optional<int> sellerPaymentAddressId = SaveUserAddress(sellerPaymentAddress);
                if (!sellerPaymentAddressId.HasValue) {
                    return false;
                }

                ebayUserData.SellerInfoSellerPaymentAddressId = sellerPaymentAddressId.GetValue();
            }

            using (var sqlConnection = GetOpenedSqlConnection2()) {
                Optional<SqlCommand> cmd = GetInsertCommand(ebayUserData, sqlConnection.SqlConnection(), "MP_EbayUserData", null, SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return false;
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the user address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Id of address</returns>
        public Optional<int> SaveUserAddress(EbayUserAddressData address) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                Optional<SqlCommand> cmd = GetInsertCommand(address, sqlConnection.SqlConnection(), "MP_EbayUserAddressData", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return Optional<int>.Empty();
                }

                using (var sqlCommand = cmd.GetValue()) {
                    int res = ExecuteScalarAndLog<int>(sqlCommand);
                    if (res > 0) {
                        return Optional<int>.Of(res);
                    }

                    return Optional<int>.Empty();
                }
            }
        }

        /// <summary>
        /// Saves the orders.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="orderInfos">The order infos.</param>
        /// <returns></returns>
        public bool SaveOrders(EbayOrder order, IEnumerable<EbayOrderInfo> orderInfos) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                Optional<SqlCommand> cmd = GetInsertCommand(order, sqlConnection.SqlConnection(), "MP_EbayOrder", "Id");
                if (!cmd.HasValue) {
                    return false;
                }

                int orderId;

                using (var sqlCommand = cmd.GetValue()) {
                    orderId = ExecuteScalarAndLog<int>(sqlCommand);
                    if (orderId < 1) {
                        return false;
                    }
                }

                return SaveOrderItems(orderInfos, orderId);
            }
        }

        /// <summary>
        /// Saves the feedback.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        /// <param name="feedbackItems">The feedback items.</param>
        /// <param name="ratings">The ratings.</param>
        /// <returns></returns>
        public bool SaveFeedbacks(EbayFeedbackData feedback, IEnumerable<EbayFeedbackItem> feedbackItems, IEnumerable<EbayRatingData> ratings) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(feedback, connection.SqlConnection(), "MP_EbayFeedback", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return false;
                }

                int feedbackId = -1;
                using (var sqlCommand = cmd.GetValue()) {
                    feedbackId = ExecuteScalarAndLog<int>(sqlCommand);
                    if (feedbackId < 1) {
                        return false;
                    }
                }

                if (!SaveFeedbackItems(feedbackItems, feedbackId, connection.SqlConnection())) {
                    return false;
                }

                return SaveRatings(ratings, feedbackId, connection.SqlConnection());
            }
        }

        /// <summary>
        /// Saves the ratings.
        /// </summary>
        /// <param name="ratings">The ratings.</param>
        /// <param name="feedbackId">The feedback identifier.</param>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <returns></returns>
        private bool SaveRatings(IEnumerable<EbayRatingData> ratings, int feedbackId, SqlConnection sqlConnection) {
            Optional<SqlCommand> cmd = GetMultiInsertCommand(ratings.Select(o => {
                o.EbayFeedbackId = feedbackId;
                return o;
            }), sqlConnection, "MP_EbayRaitingItem", SkipColumns("Id"));

            if (!cmd.HasValue)
                return false;

            using (var sqlCommand = cmd.GetValue())
                return ExecuteNonQueryAndLog(sqlCommand);
        }

        /// <summary>
        /// Saves the feedback items.
        /// </summary>
        /// <param name="feedbackItems">The feedback items.</param>
        /// <param name="feedbackId">The feedback identifier.</param>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <returns></returns>
        private bool SaveFeedbackItems(IEnumerable<EbayFeedbackItem> feedbackItems, int feedbackId, SqlConnection sqlConnection) {
            Optional<SqlCommand> cmd = GetMultiInsertCommand(feedbackItems.Select(o => {
                o.EbayFeedbackId = feedbackId;
                return o;
            }), sqlConnection, "MP_EbayFeedbackItem", SkipColumns("Id"));

            if (!cmd.HasValue)
                return false;

            using (var sqlCommand = cmd.GetValue()) {
                if (!ExecuteNonQueryAndLog(sqlCommand))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the additional user accounts.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="userAccountId">The user account identifier.</param>
        /// <param name="additionalAccounts">The additional accounts.</param>
        /// <returns></returns>
        private bool SaveAdditionalUserAccounts(SqlConnection sqlConnection, int userAccountId, IEnumerable<EbayAdditionalUserAccountData> additionalAccounts) {
            var insertCommand = GetMultiInsertCommand(additionalAccounts.Select(o => {
                o.EbayUserAccountDataId = userAccountId;
                return o;
            }), sqlConnection, "MP_EbayUserAdditionalAccountData", SkipColumns("Id"));
            if (!insertCommand.HasValue) {
                return false;
            }
            using (var sqlCommand = insertCommand.GetValue()) {
                return ExecuteNonQueryAndLog(sqlCommand);
            }
        }

        /// <summary>
        /// Saves the orders.
        /// </summary>
        /// <param name="ebayOrders">The ebay orders.</param>
        /// <param name="ebayOrderId">The ebay order identifier.</param>
        /// <returns></returns>
        private bool SaveOrderItems(IEnumerable<EbayOrderInfo> ebayOrders, int ebayOrderId) {
            if (!ebayOrders.Any(o => SaveSingleOrderInfo(o, ebayOrderId))) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the single order information.
        /// </summary>
        /// <param name="orderInfo">The order information.</param>
        /// <param name="ebayOrderId">The ebay order identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private bool SaveSingleOrderInfo(EbayOrderInfo orderInfo, int ebayOrderId) {
            Optional<int> shippingAddress = SaveUserAddress(orderInfo.ShippingAddress);
            if (!shippingAddress.HasValue) {
                return false;
            }

            orderInfo.OrderItem.ShippingAddressId = shippingAddress.GetValue();
            orderInfo.OrderItem.OrderId = ebayOrderId;

            int orderItemId = -1;

            using (var connection = GetOpenedSqlConnection2()) {
                var command = GetInsertCommand(orderInfo.OrderItem, connection.SqlConnection(), "MP_EbayOrderItem", "Id", SkipColumns("Id"));
                if (command.HasValue) {
                    using (var sqlCommand = command.GetValue()) {
                        orderItemId = ExecuteScalarAndLog<int>(sqlCommand);
                        if (orderItemId < 1) {
                            return false;
                        }
                    }
                }

                if (CollectionUtils.IsNotEmpty(orderInfo.Transactions)) {
                    if (!SaveTransactions(connection.SqlConnection(), orderInfo.Transactions, orderItemId)) {
                        return false;
                    }
                }

                if (CollectionUtils.IsNotEmpty(orderInfo.ExternalTransactions)) {
                    if (!SaveExternalTransactions(connection.SqlConnection(), orderInfo.ExternalTransactions, orderItemId)) {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Saves the transactions.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="transactions">The transactions.</param>
        /// <param name="orderItemId">The order item identifier.</param>
        /// <returns></returns>
        private bool SaveTransactions(SqlConnection connection, IEnumerable<EbayTransaction> transactions, int orderItemId) {
            var insertCommand = GetMultiInsertCommand(transactions.Select(t => {
                t.OrderItemId = orderItemId;
                return t;
            }), connection, "MP_EbayTransaction", SkipColumns("Id"));

            if (insertCommand.HasValue) {
                return ExecuteNonQueryAndLog(insertCommand.GetValue());
            }

            return true;
        }

        /// <summary>
        /// Saves the external transactions.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="transactions">The transactions.</param>
        /// <param name="orderItemId">The order item identifier.</param>
        /// <returns></returns>
        private bool SaveExternalTransactions(SqlConnection connection, IEnumerable<EbayExternalTransaction> transactions, int orderItemId) {
            var insertCommand = GetMultiInsertCommand(transactions.Select(t => {
                t.OrderItemId = orderItemId;
                return t;
            }), connection, "MP_EbayExternalTransaction", SkipColumns("Id"));

            if (insertCommand.HasValue) {
                return ExecuteNonQueryAndLog(insertCommand.GetValue());
            }

            return true;
        }
    }
}
