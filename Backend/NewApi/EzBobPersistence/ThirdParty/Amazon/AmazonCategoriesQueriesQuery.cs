using System.Collections.Generic;

namespace EzBobPersistence.ThirdParty.Amazon {
    using System.Linq;
    using EzBobCommon;
    using EzBobModels.Amazon;
    using EzBobPersistence.QueryGenerators;

    public class AmazonCategoriesQueriesQuery : QueryBase, IAmazonCategoriesQueries {
        //look at MP_MarketplaceType
        private static readonly int AmazonMarketplaceTypeId = 2;

        public AmazonCategoriesQueriesQuery(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Upserts the categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        public IDictionary<AmazonProductCategory, int> UpsertCategories(IEnumerable<AmazonProductCategory> categories) {
            Dictionary<AmazonProductCategory, int> categoryToId = new Dictionary<AmazonProductCategory, int>();
            foreach (var category in categories) {
                UpsertSingleCategory(category, categoryToId);
            }

            return categoryToId;
        }

        /// <summary>
        /// Saves the category order details mapping.
        /// </summary>
        /// <param name="maps">The maps.</param>
        /// <returns></returns>
        public bool SaveCategoryOrderDetailsMapping(IEnumerable<AmazonCategoryToOrderDetailsMap> maps) {
            using (var connection = GetOpenedSqlConnection2()) {

                return new MultiInsertCommandGenerator<AmazonCategoryToOrderDetailsMap>()
                    .WithModels(maps)
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithSkipColumns(o => o.Id)
                    .WithTableName("MP_AmazonOrderItemDetailCatgory") //this is the name of table, including typo!
                    .Verify()
                    .GenerateCommands()
                    .MapNonqueryExecuteToBool()
                    .All(o => o);
            }
        }

        /// <summary>
        /// Upserts the single category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="categoryToId">The category to identifier.</param>
        /// <returns>upserted category's id</returns>
        private int UpsertSingleCategory(AmazonProductCategory category, IDictionary<AmazonProductCategory, int> categoryToId) {
            using (var connection = GetOpenedSqlConnection2()) {
                int parentId = -1;
                if (category.Parent != null) {
                    parentId = UpsertSingleCategory(category.Parent, categoryToId);
                }

                AmazonCategory cat = new AmazonCategory {
                    MarketplaceTypeId = AmazonMarketplaceTypeId,
                    ServiceCategoryId = category.CategoryId,
                    Name = category.CategoryName,
                    ParentId = parentId > 0 ? parentId : (int?)null
                };

                var upsertCommand = GetUpsertGenerator(cat)
                    .WithConnection(connection.SqlConnection())
                    .WithSkipColumns(o => o.Id)
                    .WithOutputColumns(o => o.Id)
                    .WithTableName("MP_EbayAmazonCategory")
                    .WithMatchColumns(o => o.Name)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = upsertCommand) {
                    int id = (int)ExecuteScalarAndLog<int>(sqlCommand);
                    categoryToId.Add(category, id);
                    return id;
                }
            }
        }
    }
}
