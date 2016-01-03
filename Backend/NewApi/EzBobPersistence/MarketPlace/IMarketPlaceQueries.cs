namespace EzBobPersistence.MarketPlace {
    using System;
    using System.Collections.Generic;
    using EzBobCommon;
    using EzBobModels.MarketPlace;

    public interface IMarketPlaceQueries {
        IList<CustomerMarketPlace> GetCustomerMarketPlaces(int customerId, Guid marketplaceType);

        /// <summary>
        /// Determines whether the specified market place is exists
        /// </summary>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">empty display name</exception>
        bool IsMarketPlaceExists(Guid marketPlaceTypeId, string displayName);

        /// <summary>
        /// Determines whether the market place is in white list
        /// </summary>
        /// <param name="MarketPlaceTypeId">The market place type identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        bool IsMarketPlaceInWhiteList(Guid MarketPlaceTypeId, string token);

        /// <summary>
        /// Gets the market place identifier from marketPlaceType.
        /// </summary>
        /// <param name="marketPlaceType">Type of the market place.</param>
        /// <returns></returns>
        Optional<int> GetMarketPlaceIdFromTypeId(Guid MarketPlaceTypeId);

        /// <summary>
        /// Upserts the market place.
        /// </summary>
        /// <param name="marketPlace">The market place.</param>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <returns>marketplace's id</returns>
        int UpsertMarketPlace(CustomerMarketPlace marketPlace, Guid marketPlaceTypeId);

        /// <summary>
        /// Upserts the market place updating updateHistory.
        /// </summary>
        /// <param name="updateHistory">The updateHistory.</param>
        /// <returns></returns>
        int UpsertMarketPlaceUpdatingHistory(CustomerMarketPlaceUpdateHistory updateHistory);

        /// <summary>
        /// Gets the market place updateHistory by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        CustomerMarketPlaceUpdateHistory GetMarketPlaceUpdatingHistoryById(int id);
    }
}
 