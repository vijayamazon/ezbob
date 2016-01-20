namespace EzBobPersistence.MarketPlace {
    using System;
    using System.Collections.Generic;
    using EzBobCommon;
    using EzBobModels.MarketPlace;

    public interface IMarketPlaceQueries {
        IList<CustomerMarketPlace> GetCustomerMarketPlaces(int customerId, Guid marketplaceType);

        /// <summary>
        /// Validates the customer market place.
        /// </summary>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        InfoAccumulator ValidateCustomerMarketPlace(Guid marketPlaceTypeId, string displayName);

        /// <summary>
        /// Creates the new market place.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="securityData">The security data.</param>
        /// <param name="marketPlaceInternalId">The market place internal identifier.</param>
        /// <returns></returns>
        Optional<int> CreateNewMarketPlace(int customerId, string displayName, byte[] securityData, Guid marketPlaceInternalId);

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
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        bool IsMarketPlaceInWhiteList(Guid MarketPlaceTypeId, string displayName);

        /// <summary>
        /// Gets the market place identifier from marketPlaceType.
        /// </summary>
        /// <param name="MarketPlaceTypeId">The market place type identifier.</param>
        /// <returns></returns>
        Optional<int> GetMarketPlaceIdFromTypeId(Guid MarketPlaceTypeId);

        /// <summary>
        /// Upserts the market place.
        /// </summary>
        /// <param name="marketPlace">The market place.</param>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        Optional<int> UpsertMarketPlace(CustomerMarketPlace marketPlace, Guid marketPlaceTypeId);

        /// <summary>
        /// Upserts the market place updating updateHistory.
        /// </summary>
        /// <param name="updateHistory">The updateHistory.</param>
        /// <returns></returns>
        Optional<int> UpsertMarketPlaceUpdatingHistory(CustomerMarketPlaceUpdateHistory updateHistory);

        /// <summary>
        /// Gets the market place updateHistory by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        CustomerMarketPlaceUpdateHistory GetMarketPlaceUpdatingHistoryById(int id);
    }
}
 