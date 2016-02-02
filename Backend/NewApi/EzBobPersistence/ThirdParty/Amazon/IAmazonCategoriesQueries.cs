using System.Collections.Generic;

namespace EzBobPersistence.ThirdParty.Amazon {
    using EzBobModels.Amazon;

    public interface IAmazonCategoriesQueries {
        /// <summary>
        /// Upserts the categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        IDictionary<AmazonProductCategory, int> UpsertCategories(IEnumerable<AmazonProductCategory> categories);

        bool SaveCategoryOrderDetailsMapping(IEnumerable<AmazonCategoryToOrderDetailsMap> maps);
    }
}
