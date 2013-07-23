namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using Marketplaces.FreeAgent;
	using Marketplaces.Sage;
	using Marketplaces.Yodlee;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using DatabaseWrapper;
	using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using NHibernate;
	using NHibernate.Linq;

    public interface ICustomerMarketPlaceRepository : IRepository<MP_CustomerMarketPlace>
    {
        bool Exists(Customer customer, MP_MarketplaceType marketplaceType);
        IEnumerable<MP_CustomerMarketPlace> Get(Customer customer, MP_MarketplaceType marketplaceType);
        MP_CustomerMarketPlace Get(int customerId, Guid marketPlaceInternalId, string marketPlaceName);
        bool Exists(Guid marketplace, string displayName);
        bool Exists(Guid marketplace, Customer customer, string displayName);
        DateTime? GetLastAmazonOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastTeraPeakOrdersRequestData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastInventoryRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastEbayOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastPayPalTransactionRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        MP_CustomerMarketPlace GetMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        string GetUpdatedStatus(int marketplacId);
        void ClearUpdatingEnd(int marketplaceId);
    }

    public class CustomerMarketPlaceRepository : NHibernateRepositoryBase<MP_CustomerMarketPlace>, ICustomerMarketPlaceRepository
    {
        public CustomerMarketPlaceRepository(ISession session)
            : base(session)
        {
        }

        public bool Exists(Customer customer, MP_MarketplaceType marketplaceType)
        {
            return GetAll().Any(cm => cm.Customer.Id == customer.Id && cm.Marketplace.Id == marketplaceType.Id);
        }

        public IEnumerable<MP_CustomerMarketPlace> Get(Customer customer, MP_MarketplaceType marketplaceType)
        {
            return GetAll().Where(cm => cm.Customer.Id == customer.Id && cm.Marketplace.Id == marketplaceType.Id).ToList();
        }

        public MP_CustomerMarketPlace Get(int customerId, Guid marketPlaceInternalId, string marketPlaceName)
        {
            return GetAll().FirstOrDefault(cmp => cmp.Customer.Id == customerId && cmp.Marketplace.InternalId == marketPlaceInternalId && cmp.DisplayName.Equals(marketPlaceName));
        }

        public bool Exists(Guid marketplace, string displayName)
        {
            return _session
                .QueryOver<MP_CustomerMarketPlace>()
                .Where(m => m.DisplayName == displayName)
                .JoinQueryOver(m => m.Marketplace)
                .Where(m => m.InternalId == marketplace)
                .RowCount() != 0;
        }

        public bool Exists(Guid marketplace, Customer customer, string displayName)
        {
            return _session
                .QueryOver<MP_CustomerMarketPlace>()
                .Where(m => m.Customer.Id == customer.Id && m.DisplayName == displayName)
                .JoinQueryOver(m => m.Marketplace)
                .Where(m => m.InternalId == marketplace)
                .RowCount() != 0;
        }

        public DateTime? GetLastAmazonOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
            if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.AmazonOrders.Count == 0)
            {
                return null;
            }
            return mpCustomerMarketPlace.AmazonOrders.Max(o => o.Created);
        }

	    public TeraPeakDatabaseSellerData GetAllTeraPeakDataWithFullRange(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
	    {
			var customerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);

			var data = new TeraPeakDatabaseSellerData(submittedDate);

			data.AddRange(customerMarketPlace.TeraPeakOrders
				.SelectMany(to => to.OrderItems)
				.Where(oi => oi.RangeMarker == RangeMarkerType.Full)
				.Select(o =>
							new TeraPeakDatabaseSellerDataItem(o.StartDate, o.EndDate)
							{
								AverageSellersPerDay = o.AverageSellersPerDay,
								Bids = o.Bids,
								ItemsOffered = o.ItemsOffered,
								ItemsSold = o.ItemsSold,
								Listings = o.Listings,
								Revenue = o.Revenue,
								SuccessRate = o.SuccessRate,
								Successful = o.Successful,
								Transactions = o.Transactions,
								RangeMarker = o.RangeMarker
							}
						)
				);

			return data;
	    }

	    public DateTime? GetLastTeraPeakOrdersRequestData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
            if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.TeraPeakOrders.Count == 0)
            {
                return null;
            }
            return mpCustomerMarketPlace.TeraPeakOrders.Max(o => o.Created);
        }

        public DateTime? GetLastInventoryRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
            if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.Inventory.Count == 0)
            {
                return null;
            }
            return mpCustomerMarketPlace.Inventory.Max(o => o.Created);
        }

        public DateTime? GetLastEbayOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
            if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.EbayOrders.Count == 0)
            {
                return null;
            }
            return mpCustomerMarketPlace.EbayOrders.Max(o => o.Created);
        }

        public DateTime? GetLastPayPalTransactionRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
            if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.PayPalTransactions.Count == 0)
            {
                return null;
            }
            return mpCustomerMarketPlace.PayPalTransactions.Max(o => o.Created);
        }

        public MP_CustomerMarketPlace GetMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            //return GetAll().FirstOrDefault(cmp => cmp.Id == databaseCustomerMarketPlace.Id);
	        return Get(databaseCustomerMarketPlace.Id);
        }

        public string GetUpdatedStatus(int marketplacId)
        {
            var mp = Get(marketplacId);
            return 
                (mp.UpdatingStart != null && mp.UpdatingEnd == null) ? "In progress" :
                (!String.IsNullOrEmpty(mp.UpdateError)) ? "Error" : "Done";
        }

        public void ClearUpdatingEnd(int marketplaceId)
        {
            var mp = Get(marketplaceId);
            mp.UpdatingEnd = null;
            Update(mp);
        }
    }


}