using System;

namespace EzBobPersistence.ThirdParty.Amazon.QueryBuilders {
    using System.Data;
    using System.Data.SqlClient;
    using EzBobPersistence.QueryGenerators;

    /// <summary>
    /// Creates get top N order items command
    /// </summary>
    internal class AmazonTopNOrderItemsCommandBuilder {
        private static readonly string Query = @";WITH shipped AS (
                                                        SELECT
                                                            oi.Id, oi.OrderTotalCurrency, oi.OrderTotal, oi.NumberOfItemsShipped
                                                        FROM
                                                            MP_AmazonOrderItem oi
                                                            INNER JOIN MP_AmazonOrder o ON oi.AmazonOrderId = o.Id AND o.CustomerMarketPlaceId = @mpId
                                                        WHERE
                                                            OrderStatus LIKE 'shipped'
                                                    ), grp AS (
                                                        SELECT TOP @topN
                                                            OrderTotal, OrderTotalCurrency, NumberOfItemsShipped, Counter = Count(*)
                                                        FROM
                                                            shipped
                                                        GROUP BY
                                                            OrderTotal, OrderTotalCurrency, NumberOfItemsShipped
                                                        ORDER BY
                                                            4 DESC
                                                    ), ids AS (
                                                        SELECT
                                                            Id = MAX(s.Id)
                                                        FROM
                                                            grp g
                                                            INNER JOIN shipped s
                                                                ON g.OrderTotal = s.OrderTotal
                                                                AND g.OrderTotalCurrency = s.OrderTotalCurrency
                                                                AND g.NumberOfItemsShipped = s.NumberOfItemsShipped
                                                        GROUP BY
                                                            g.OrderTotal, g.OrderTotalCurrency, g.NumberOfItemsShipped
                                                    ) SELECT
                                                        oi.*
                                                    FROM
                                                        MP_AmazonOrderItem oi
                                                        INNER JOIN ids ON oi.Id = ids.Id";
        private int marketPlaceId = -1;
        private int topN = 10;
        private SqlConnection connection;

        /// <summary>
        /// Helps to build fluent syntax
        /// </summary>
        public class QueryBuilder : ISqlCommandGenerator {
            private readonly AmazonTopNOrderItemsCommandBuilder builder;

            public QueryBuilder(AmazonTopNOrderItemsCommandBuilder builder) {
                this.builder = builder;
            }

            public SqlCommand GenerateCommand() {
                return this.builder.Generate();
            }
        }

        /// <summary>
        /// Sets the marketplace identifier.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <returns></returns>
        public AmazonTopNOrderItemsCommandBuilder WithMarketPlaceId(int marketPlaceId) {
            this.marketPlaceId = marketPlaceId;
            return this;
        }

        /// <summary>
        /// Sets the optional top topN - how many top orders to return.
        /// </summary>
        /// <param name="topN">The topN.</param>
        /// <returns></returns>
        public AmazonTopNOrderItemsCommandBuilder WithOptionalTopN(int topN = 10) {
            this.topN = topN;
            return this;
        }

        /// <summary>
        /// Sets the optional connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public AmazonTopNOrderItemsCommandBuilder WithOptionalConnection(SqlConnection connection) {
            this.connection = connection;
            return this;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// invalid marketplace Id
        /// or
        /// invalid top N
        /// </exception>
        public ISqlCommandGenerator Verify() {
            if (this.marketPlaceId < 1) {
                throw new ArgumentException("invalid marketplace Id");
            }

            if (this.topN < 1) {
                throw new ArgumentException("invalid top N");
            }

            return new QueryBuilder(this);
        }

        /// <summary>
        /// Generates the sql command.
        /// </summary>
        /// <returns></returns>
        private SqlCommand Generate() {
            SqlCommand sqlCommand = new SqlCommand();
            if (this.connection != null) {
                sqlCommand.Connection = this.connection;
            }
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandText = Query;
            sqlCommand.Parameters.AddWithValue("@mpId", this.marketPlaceId);
            sqlCommand.Parameters.AddWithValue("@topN", this.topN);
            return sqlCommand;
        }
    }
}
