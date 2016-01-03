namespace EzBobPersistence.Yodlee {
    using System.Collections.Generic;
    using EzBobModels.Yodlee;

    public interface IYodleeQueries {
        /// <summary>
        /// Gets the user's account.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        YodleeUserAccount GetUserAccount(int customerId);

        /// <summary>
        /// Books the user account.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        YodleeUserAccount BookUserAccount(int customerId);

        /// <summary>
        /// This function exists to bridge between new yodlee api and old yodlee api.<br/>
        /// 'ContentServiceId' is the same in both apis.<br/> 
        /// But new api contains a new element: 'Site' - which contains 'content services'
        /// <br/>
        /// Gets the site identifier from content service identifier.
        /// </summary>
        /// <param name="contentServiceId">The content service identifier.</param>
        /// <returns></returns>
        int? GetSiteIdFromContentServiceId(int contentServiceId);

        /// <summary>
        /// Determines whether the user already have the specified content service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="contentServiceId">The content service identifier.</param>
        /// <returns></returns>
        bool IsUserAlreadyHaveContentService(int customerId, int contentServiceId);

        /// <summary>
        /// Gets the user content services.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        IEnumerable<int> GetUserContentServicesIds(int customerId);

        /// <summary>
        /// Saves the user account.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool? UpsertContentServiceAccount(YodleeOrderItem item);

        /// <summary>
        /// Saves the transactions.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <returns></returns>
        IList<bool?> UpsertTransactions(IEnumerable<YodleeOrderItemTransaction> transactions);
    }
}
