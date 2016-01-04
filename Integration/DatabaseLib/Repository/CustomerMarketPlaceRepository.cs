namespace EZBob.DatabaseLib.Model.Database.Repository {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using DatabaseWrapper;
    using Ezbob.Database;
    using Ezbob.Logger;
    using NHibernate;
    using log4net;

	public interface ICustomerMarketPlaceRepository : IRepository<MP_CustomerMarketPlace>
    {
        bool Exists(Customer customer, MP_MarketplaceType marketplaceType);
        IEnumerable<MP_CustomerMarketPlace> Get(Customer customer, MP_MarketplaceType marketplaceType);
        MP_CustomerMarketPlace Get(int customerId, Guid marketPlaceInternalId, string marketPlaceName);
        bool Exists(Guid marketplace, int originID, string displayName);
        bool Exists(Guid marketplace, Customer customer, string displayName);
        DateTime? GetLastAmazonOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastEbayOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        DateTime? GetLastPayPalTransactionRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
        MP_CustomerMarketPlace GetMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace);
    }

    public class CustomerMarketPlaceRepository : NHibernateRepositoryBase<MP_CustomerMarketPlace>, ICustomerMarketPlaceRepository
    {
		public CustomerMarketPlaceRepository(ISession session) : base(session) {
			m_oRetryer = new SqlRetryer(
				3,
				500,
				new SafeILog(LogManager.GetLogger(typeof(CustomerMarketPlaceRepository)))
			);
		}

		public override void Update(MP_CustomerMarketPlace val) {
			m_oRetryer.Retry(() => base.Update(val));
		}

        public bool Exists(Customer customer, MP_MarketplaceType marketplaceType)
        {
            return GetAll().Any(cm => cm.Customer.Id == customer.Id && cm.Marketplace.Id == marketplaceType.Id);
        }

        public IEnumerable<MP_CustomerMarketPlace> GetAll(Customer customer)
        {
            return GetAll().Where(cm => cm.Customer.Id == customer.Id ).ToList();
        }

		public IEnumerable<MP_CustomerMarketPlace> GetAllByCustomer(int customerId)
		{
			return GetAll().Where(cm => cm.Customer.Id == customerId).ToList();
		}

        public IEnumerable<MP_CustomerMarketPlace> Get(Customer customer, MP_MarketplaceType marketplaceType)
        {
            return GetAll().Where(cm => cm.Customer.Id == customer.Id && cm.Marketplace.Id == marketplaceType.Id).ToList();
        }

        public MP_CustomerMarketPlace Get(int customerId, Guid marketPlaceInternalId, string marketPlaceName)
        {
            return GetAll().FirstOrDefault(cmp => cmp.Customer.Id == customerId && cmp.Marketplace.InternalId == marketPlaceInternalId && cmp.DisplayName.Equals(marketPlaceName));
        }

		public bool Exists(Guid marketplace, int originID, string displayName) {
			return Library.Instance.DB.ExecuteScalar<bool>(
				"CheckMarketplaceExists",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpTypeID", marketplace),
				new QueryParameter("OriginID", originID),
				new QueryParameter("Token", displayName)
			);
		} // Exists

        public bool Exists(Guid marketplace, Customer customer, string displayName)
        {
            return Session
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

		public DateTime? GetLastAmazonOrderDate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
			if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.AmazonOrders.Count == 0) {
				return null;
			}
			var orderDates = mpCustomerMarketPlace
				.AmazonOrders
				.SelectMany(x => x.OrderItems)
				.Select(x => x.LastUpdateDate)
				.ToList();

			return orderDates.Any() ? orderDates.Max() : (DateTime?)null;
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

		public DateTime? GetLastPayPalTransactionDate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			var mpCustomerMarketPlace = GetMarketPlace(databaseCustomerMarketPlace);
			if (mpCustomerMarketPlace == null || mpCustomerMarketPlace.PayPalTransactions.Count == 0) {
				return null;
			}

			var transactionsDates = mpCustomerMarketPlace.PayPalTransactions.SelectMany(x => x.TransactionItems).Select(x => x.Created).ToList();
			return transactionsDates.Any() ? transactionsDates.Max() : (DateTime?)null;
		}

        public MP_CustomerMarketPlace GetMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
        {
            //return GetAll().FirstOrDefault(cmp => cmp.Id == databaseCustomerMarketPlace.Id);
	        return Get(databaseCustomerMarketPlace.Id);
        }

	    private SqlRetryer m_oRetryer;
    }
}