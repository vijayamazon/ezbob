using System.Collections.Generic;

namespace EzBobPersistence.ThirdParty.Amazon {
    using System.Linq;
    using EzBobModels.Amazon;
    using EzBobPersistence.QueryGenerators;
    using EzBobPersistence.ThirdParty.Amazon.QueryBuilders;

    internal class AmazonOrderDetailsQueries : QueryBase, IAmazonOrderDetailsQueries {
        public AmazonOrderDetailsQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Saves the order details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>
        /// order details filled with id and sellerSku
        /// </returns>
        public IEnumerable<AmazonOrderItemDetail> SaveOrderDetails(IEnumerable<AmazonOrderItemDetail> details) {
            using (var connection = GetOpenedSqlConnection2()) {
                return new MultiInsertCommandGenerator<AmazonOrderItemDetail>()
                    .WithModels(details)
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithSkipColumns(o => o.Id)
                    .WithOutputColumns(o => o.Id, o => o.SellerSKU)
                    .WithTableName("MP_AmazonOrderItemDetail")
                    .Verify()
                    .GenerateCommands() //generated sql commands
                    .MapToModels<AmazonOrderItemDetail>(); //executes commands (command per batch) and maps into model's collection
            }
        }

        public IEnumerable<string> GetCategoriesCategoriesBySku(string sellerSku) {
            using (var connection = GetOpenedSqlConnection2()) {
                return new CategoriesBySkuCommandBuilder()
                    .WithConnection(connection.SqlConnection())
                    .WithSku(sellerSku)
                    .Verify()
                    .GenerateCommand()
                    .MapToStringCollection()
                    .ToArray();
            }
        }
    }
}
