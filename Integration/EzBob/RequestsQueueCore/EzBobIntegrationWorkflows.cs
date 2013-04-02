using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Exceptions;
using EZBob.DatabaseLib.Model.Database;
using EzBob.RequestsQueueCore.RequestInfos;
using StructureMap;

namespace EzBob.RequestsQueueCore
{
	internal class EzBobIntegrationWorkflows : EzBobIntegrationWorkflowsBase
	{
		private static IEzBobIntegrationWorkflows _Instance;
		private static IEnumerable<IDatabaseMarketplace> _RegisteredMarketplaces;

		private static IEnumerable<IDatabaseMarketplace> RegisteredMarketplaces
		{
			get { return _RegisteredMarketplaces ?? ( _RegisteredMarketplaces = GetRegisteredMarketplaces() ); }
		}

		public static IEzBobIntegrationWorkflows Instance
		{
			get { return _Instance ?? ( _Instance = new EzBobIntegrationWorkflows() ); }
		}

		public override int UpdateCustomerData( int customerId )
		{
			IEnumerable<IDatabaseMarketplace> mpList = RegisteredMarketplaces;

			var helper = Helper;
			var customer = helper.GetCustomerInfo( customerId );

			var listMpId = new List<IDatabaseCustomerMarketPlace>();

			foreach (IDatabaseMarketplace mp in mpList)
			{
				var list = helper.GetCustomerMarketPlaceList( customer, mp );

				if ( list != null && list.Any() )
				{
					listMpId.AddRange( list );
				}
			}


			IEnumerable<IRequestData> infoList = listMpId.Select( mp =>
				{
					return RequestInfoFactory.CreateSingleRequest( mp.Marketplace, () =>
					{
						var databaseDataHelper = Helper;
						var retrieveDataHelper = mp.Marketplace.GetRetrieveDataHelper( databaseDataHelper );
						retrieveDataHelper.UpdateCustomerMarketplaceRegular( mp.Id );
					} );
				} );

			/*IEnumerable<IRequestData> infoList = mpList.Select( mp =>
				
					RequestInfoFactory.CreateSingleRequest( mp, () => 
						{
							var databaseDataHelper = Helper;
							var retrieveDataHelper = mp.GetRetrieveDataHelper( databaseDataHelper );
							try
							{
								retrieveDataHelper.UpdateCustomer( customerId );
							}
							catch ( NoMarketPlaceForCustomerException )
							{

							}
						})
				 );
			*/
			IRequestData info = RequestInfoFactory.CreateCompositeRequest( infoList );

			return CreateRequest( info );
		}

		public override int UpdateCustomerMarketPlaceData( int customerMarketPlaceId )
		{
			IDatabaseMarketplace mp = GetRegisteredMarketPlace( customerMarketPlaceId );
			IRequestData requestinfo = RequestInfoFactory.CreateSingleRequest( mp, () =>
				{
					var databaseDataHelper = Helper;
					var retrieveDataHelper = mp.GetRetrieveDataHelper( databaseDataHelper );
					retrieveDataHelper.UpdateCustomerMarketplaceFirst( customerMarketPlaceId );
				} );
			return CreateRequest( requestinfo );
		}

		private IRequestData UpdateCustomerMarketPlaceData( IDatabaseMarketplace mp, int customerMarketPlaceId )
		{
			return RequestInfoFactory.CreateSingleRequest( mp, () =>
			{
				var databaseDataHelper = Helper;
				var retrieveDataHelper = mp.GetRetrieveDataHelper( databaseDataHelper );
				retrieveDataHelper.UpdateCustomerMarketplaceFirst( customerMarketPlaceId );
			} );
		}

		public override IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId)
		{
			IMarketplaceRetrieveDataHelper retrieveDataHelper = GetMarketplaceRetrieveDataHelper( customerMarketPlaceId );
			return retrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace( customerMarketPlaceId );
		}

		private IMarketplaceRetrieveDataHelper GetMarketplaceRetrieveDataHelper( int customerMarketPlaceId )
		{
			IDatabaseMarketplace mp = GetRegisteredMarketPlace( customerMarketPlaceId );
			DatabaseDataHelper databaseDataHelper = Helper;
			return mp.GetRetrieveDataHelper( databaseDataHelper );			
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			var retrieveDataHelper = GetMarketplaceRetrieveDataHelper( customerMarketPlaceId );

			return retrieveDataHelper.RetrieveCustomerSecurityInfo( customerMarketPlaceId );
		}

		private MP_CustomerMarketPlace GetCustomerMarketPlace( int customerMarketPlaceId )
		{
			MP_CustomerMarketPlace cmp = Helper.GetCustomerMarketPlace( customerMarketPlaceId );

			if ( cmp == null )
			{
				throw new InvalidCustomerMarketPlaceException( customerMarketPlaceId );
			}
			return cmp;
		}

		private IDatabaseMarketplace GetRegisteredMarketPlace( MP_MarketplaceType mp )
		{
			return ObjectFactory.GetNamedInstance<IDatabaseMarketplace>( mp.Name );
		}

		private IDatabaseMarketplace GetRegisteredMarketPlace( int customerMarketPlaceId )
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace( customerMarketPlaceId );

			return GetRegisteredMarketPlace( customerMarketPlace.Marketplace );
		}

		private static IEnumerable<IDatabaseMarketplace> GetRegisteredMarketplaces()
		{
			return ObjectFactory.GetAllInstances<IDatabaseMarketplace>();
		}

		private DatabaseDataHelper Helper
		{
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		}

		
	}
}