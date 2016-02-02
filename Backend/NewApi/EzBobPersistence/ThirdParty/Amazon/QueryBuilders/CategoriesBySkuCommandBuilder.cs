using System;

namespace EzBobPersistence.ThirdParty.Amazon.QueryBuilders {
    using System.Data;
    using System.Data.SqlClient;
    using EzBobPersistence.QueryGenerators;

    internal class CategoriesBySkuCommandBuilder {
        private static readonly string query = @"SELECT DISTINCT (cat.Name) 
                                                 FROM 
                                                     MP_EbayAmazonCategory cat 
                                                     JOIN MP_AmazonOrderItemDetailCatgory map ON map.EbayAmazonCategoryId = cat.Id
                                                     JOIN MP_AmazonOrderItemDetail d ON map.AmazonOrderItemDetailId = d.Id AND d.SellerSKU LIKE @p";

        private string sku;
        private SqlConnection connection;

        /// <summary>
        /// Helps to make fluent syntax
        /// </summary>
        public class Generator : ISqlCommandGenerator {
            private readonly CategoriesBySkuCommandBuilder builder;

            public Generator(CategoriesBySkuCommandBuilder commandBuilder) {
                this.builder = commandBuilder;
            }

            public SqlCommand GenerateCommand() {
                return this.builder.Generate();
            }
        }

        /// <summary>
        /// Sets the sku.
        /// </summary>
        /// <param name="sellerSku">The seller sku.</param>
        /// <returns></returns>
        public CategoriesBySkuCommandBuilder WithSku(string sellerSku) {
            this.sku = sellerSku;
            return this;
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public CategoriesBySkuCommandBuilder WithConnection(SqlConnection connection) {
            this.connection = connection;
            return this;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid SKU</exception>
        public ISqlCommandGenerator Verify() {
            if (string.IsNullOrEmpty(this.sku)) {
                throw new ArgumentException("Invalid SKU");
            }

            return new Generator(this);
        }

        /// <summary>
        /// Generates this command.
        /// </summary>
        /// <returns></returns>
        private SqlCommand Generate() {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            command.Parameters.AddWithValue("@p", this.sku);
            if (this.connection != null) {
                command.Connection = this.connection;
            }

            return command;
        }
    }
}
