using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.Exceptions;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib.Common
{
	using Ezbob.Utils.Serialization;

	public abstract class MarketplaceRetrieveDataHelperBase<TEnum> : IMarketplaceRetrieveDataHelper
	{
		protected DatabaseDataHelper Helper { get; private set; }

		private readonly DatabaseMarketplaceBase<TEnum> _Marketplace;

		protected MarketplaceRetrieveDataHelperBase( DatabaseDataHelper helper, DatabaseMarketplaceBase<TEnum> marketplace )
		{
			Helper = helper;
			_Marketplace = marketplace;
		}

		protected abstract void InternalUpdateInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord );

		protected abstract void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo value);

		private IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			var rez = new AnalysisDataInfo(databaseCustomerMarketPlace, Helper.GetAnalyisisFunctions(databaseCustomerMarketPlace));

			AddAnalysisValues(databaseCustomerMarketPlace, rez);

			return rez;
		}

		private void UpdateInfoFirst( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
		{
            if (databaseCustomerMarketPlace.Disabled) return;
			Helper.UpdateCustomerMarketplaceData( databaseCustomerMarketPlace, 
									historyRecord => InternalUpdateInfoFirst( databaseCustomerMarketPlace, historyRecord ));
		}

		private void UpdateInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
		{
		    if (databaseCustomerMarketPlace.Disabled) return;
			Helper.UpdateCustomerMarketplaceData( databaseCustomerMarketPlace, 
									historyRecord => InternalUpdateInfo( databaseCustomerMarketPlace, historyRecord ));
		}

		protected virtual void InternalUpdateInfoFirst( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
			InternalUpdateInfo( databaseCustomerMarketPlace, historyRecord );
		}

		public Customer GetCustomerInfo( int customerId )
		{
			return Helper.GetCustomerInfo( customerId );
		}

		public void UpdateCustomerMarketplaceFirst(int customerMarketPlaceId)
		{
			UpdateInfoFirst( GetDatabaseCustomerMarketPlace(customerMarketPlaceId) );
		}

		public void UpdateCustomerMarketplaceRegular(int customerMarketPlaceId)
		{
			UpdateInfo( GetDatabaseCustomerMarketPlace( customerMarketPlaceId ) );
		}

		protected IDatabaseCustomerMarketPlace GetDatabaseCustomerMarketPlace( int customerMarketPlaceId )
		{
			return Helper.GetDatabaseCustomerMarketPlace( _Marketplace, customerMarketPlaceId );
		}

		public IMarketplaceType Marketplace
		{
			get { return _Marketplace; }
		}

		public IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace( int customerMarketPlaceId )
		{
			MP_CustomerMarketPlace marketPlace = Helper.GetCustomerMarketPlace( customerMarketPlaceId );
            IDatabaseCustomerMarketPlace databaseCustomerMarketPlace = Helper.CreateDatabaseCustomerMarketPlace(marketPlace.DisplayName, _Marketplace, marketPlace.Customer);

			return GetAnalysisValuesByCustomerMarketPlace( databaseCustomerMarketPlace );
		}

        public void StoreOrUpdateCustomerSecurityInfo(Customer databaseCustomer, IMarketPlaceSecurityInfo securityData, string marketPlaceName)
		{
			Helper.SaveOrUpdateCustomerMarketplace( marketPlaceName, _Marketplace, securityData, databaseCustomer );
		}

		protected void SaveOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EbayDatabaseOrdersList databaseOrdersList, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.AddEbayOrdersData( databaseCustomerMarketPlace, databaseOrdersList, historyRecord );
		}

		public abstract IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);			

		protected TSecurityData RetrieveCustomerSecurityInfo<TSecurityData>( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
			where TSecurityData : IMarketPlaceSecurityInfo
		{
			return Serialized.Deserialize<TSecurityData>( databaseCustomerMarketPlace.SecurityData );
		}
	}
}
