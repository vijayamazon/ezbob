using System.Collections.Generic;
using System.Linq;

namespace EzBob3dParties.Amazon {
    using EzBob3dParties.Amazon.Src.ProductsApi.Model;
    using EzBob3dPartiesApi.Amazon;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Amazon;
    using NServiceBus;

    public class AmazonGetProductCategoriesHandler : HandlerBase<AmazonGetProductCategories3dPartyCommandResponse>, IHandleMessages<AmazonGetProductCategories3dPartyCommand> {

        [Injected]
        internal IAmazonService AmazonService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(AmazonGetProductCategories3dPartyCommand command) {
            InfoAccumulator info = new InfoAccumulator();

            IDictionary<string, IEnumerable<AmazonProductCategory>> categoriesPerSku = new Dictionary<string, IEnumerable<AmazonProductCategory>>();

            foreach (var sku in command.SellerSKUs.Distinct()) {

                if (string.IsNullOrEmpty(sku)) {
                    Log.Warn("got empty sku");
                    continue;
                }

                var categoriesRequest = new GetProductCategoriesForSKURequest {
                    SellerId = command.SellerId,
                    SellerSKU = sku,
                    MarketplaceId = command.MarketplaceId
                };

                GetProductCategoriesForSKUResponse response = await AmazonService.Products.GetProductCategoriesForSKU(categoriesRequest);
                IEnumerable<AmazonProductCategory> categories = Enumerable.Empty<AmazonProductCategory>();
                if (response.IsSetGetProductCategoriesForSKUResult()) {
                    categories = CreateCategories(response.GetProductCategoriesForSKUResult.Self);
                }

                categoriesPerSku.Add(sku, categories);
            }

            SendReply(info, command, resp => resp.CategoriesBySku = categoriesPerSku);
        }

        /// <summary>
        /// Creates the categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        private IEnumerable<AmazonProductCategory> CreateCategories(IList<Categories> categories) {
            foreach (var cat in categories) {
                yield return CreateSingleCategory(cat);
            }
        }

        /// <summary>
        /// Creates the single category.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        private AmazonProductCategory CreateSingleCategory(Categories categories) {
            if (categories == null) {
                return null;
            }
            return new AmazonProductCategory {
                CategoryId = categories.ProductCategoryId,
                CategoryName = categories.ProductCategoryName,
                Parent = CreateSingleCategory(categories.Parent)
            };
        }
    }
}
