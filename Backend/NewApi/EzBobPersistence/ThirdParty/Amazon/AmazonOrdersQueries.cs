using System;

namespace EzBobPersistence.ThirdParty.Amazon {
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using EzBobCommon;
    using EzBobModels.Amazon;
    using EzBobModels.Amazon.Helpers;
    using EzBobPersistence.QueryGenerators;
    using EzBobPersistence.ThirdParty.Amazon.QueryBuilders;

    public class AmazonOrdersQueries : QueryBase, IAmazonOrdersQueries {
        public AmazonOrdersQueries(string connectionString)
            : base(connectionString) {}

        [Injected]
        public IAmazonCategoriesQueries Categories { get; set; }

        [Injected]
        public IAmazonOrderDetailsQueries OrderDetails { get; set; }

        public IEnumerable<int> SaveOrderDetails(IEnumerable<AmazonOrderItemDetail> details) {
            List<int> ids = new List<int>(details.Count());
            using (var connection = GetOpenedSqlConnection2()) {

                var commands = new MultiInsertCommandGenerator<AmazonOrderItemDetail>()
                    .WithModels(details)
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithSkipColumns("Id")
                    .WithOutputColumns("Id")
                    .WithTableName("MP_AmazonOrderItemDetail")
                    .Verify()
                    .GenerateCommands();

                foreach (var command in commands) {
                    using (var sqlCommand = command) {
                        ids.AddRange(CreatePrimitivesCollection<int>(sqlCommand.ExecuteReader()));
                    }    
                }
            }

            return ids;
        } 

        /// <summary>
        /// Saves the order details.
        /// </summary>
        /// <param name="ordersPayments">The orders payments.</param>
        /// <param name="amazonOrderId">The amazon order identifier.</param>
        /// <returns></returns>
        public bool SaveOrdersPayments(IEnumerable<AmazonOrderItemAndPayments> ordersPayments, int amazonOrderId) {
            bool res = true;
            foreach (var orderPayments in ordersPayments) {
                res &= SaveOrderPayments(orderPayments, amazonOrderId);
                if (!res) {
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Saves the order payments.
        /// </summary>
        /// <param name="orderPayments">The order payments.</param>
        /// <param name="amazonOrderId">The amazon order identifier.</param>
        /// <returns></returns>
        public bool SaveOrderPayments(AmazonOrderItemAndPayments orderPayments, int amazonOrderId) {
            orderPayments.OrderItem.AmazonOrderId = amazonOrderId;

            int orderItemId = (int)SaveOrderItem(orderPayments.OrderItem);
            if (orderItemId < 1) {
                return false;
            }

            bool res = true;
            foreach (var batch in orderPayments.Payments.ForEach(payment => payment.OrderItemId = orderItemId)
                .Batch(800)) {
                res &= SaveOrderItemPayments(batch);
                if (!res) {
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Saves the order item payments.
        /// </summary>
        /// <param name="payments">The payments.</param>
        /// <returns></returns>
        public bool SaveOrderItemPayments(IEnumerable<AmazonOrderItemPayment> payments) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetMultiInsertCommand(payments, connection.SqlConnection(), "MP_AmazonOrderItemPayment", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return false;
                }

                using (var sqlCommand = (SqlCommand)cmd) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the order item.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <returns></returns>
        public Optional<int> SaveOrderItem(AmazonOrderItem orderItem) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(orderItem, connection.SqlConnection(), "MP_AmazonOrderItem", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return Optional<int>.Empty();
                }

                using (var sqlCommand = (SqlCommand)cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the order.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public Optional<int> SaveOrder(AmazonOrder order) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetInsertCommand(order, connection.SqlConnection(), "MP_AmazonOrder", "Id", SkipColumns("Id"));
                if (!cmd.HasValue) {
                    return Optional<int>.Empty();
                }

                using (var sqlCommand = (SqlCommand)cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the top n order items.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="topN">The top n.</param>
        /// <returns></returns>
        public IEnumerable<AmazonOrderItem> GetTopNOrderItems(int marketPlaceId, int topN = 10) {
            using (var connection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = GenerateTopNOrderItemsCommand(marketPlaceId, topN, connection.SqlConnection())) {
                    return CreateModels<AmazonOrderItem>(sqlCommand.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// Gets the last order date.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <returns></returns>
        public Optional<DateTime> GetLastOrderDate(int marketPlaceId) {
            //TODO: review
            using (var connection = GetOpenedSqlConnection2()) {
                string query = "SELECT Max(Created) FROM MP_AmazonOrder WHERE CustomerMarketPlaceId = @MarketPlaceId";
                using (var sqlCommand = GetEmptyCommand(connection.SqlConnection())) {
                    sqlCommand.CommandText = query;
                    sqlCommand.Parameters.AddWithValue("@MarketPlaceId", marketPlaceId);
                    return ExecuteScalarAndLog<DateTime>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Generates the top n order items command.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="topN">The top n.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private SqlCommand GenerateTopNOrderItemsCommand(int marketPlaceId, int topN, SqlConnection connection) {
            SqlCommand command = new AmazonTopNOrderItemsCommandBuilder()
                .WithOptionalTopN(topN)
                .WithMarketPlaceId(marketPlaceId)
                .WithOptionalConnection(connection)
                .Verify()
                .GenerateCommand();

            return command;
        }
    }
}
