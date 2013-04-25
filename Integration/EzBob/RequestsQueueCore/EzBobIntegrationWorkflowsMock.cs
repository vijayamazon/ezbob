using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.RequestsQueueCore.RequestInfos;

namespace EzBob.RequestsQueueCore
{
	class EzBobIntegrationWorkflowsMock : EzBobIntegrationWorkflowsBase
	{
		private static IEzBobIntegrationWorkflows _Instance;

		public static IEzBobIntegrationWorkflows Instance
		{
			get { return _Instance ?? ( _Instance = new EzBobIntegrationWorkflowsMock() ); }
		}

		private readonly IMarketplaceType[] _ListMp;
		private readonly IMarketplaceType _Mp1;
		private readonly IMarketplaceType _Mp2;
		private readonly IMarketplaceType _Mp3;

		public EzBobIntegrationWorkflowsMock()
		{
			_Mp1 = new DatabaseMarketplaceTest( new MarketplaceInfoTest( "test1" ) );
			_Mp2 = new DatabaseMarketplaceTest( new MarketplaceInfoTest( "test2" ) );
			_Mp3 = new DatabaseMarketplaceTest( new MarketplaceInfoTest( "test3" ) );

			_ListMp = new[] { _Mp1, _Mp2, _Mp3 };
		}

		public override int UpdateCustomerData(int customerId)
		{		
			IEnumerable<IRequestData> infoList = _ListMp.Select( mp => RequestInfoFactory.CreateSingleRequest( mp, TestProc ));

			IRequestData info = RequestInfoFactory.CreateCompositeRequest( infoList );

			return CreateRequest( info );
			
		}

		private void TestProc()
		{
			Thread.Sleep( 1*60*1000 );
		}

		private void TestProcError()
		{
			throw new Exception( "Test Error" );
		}

		public override int UpdateCustomerMarketPlaceData(int customerMarketPlaceId)
		{
			IRequestData requestinfo = RequestInfoFactory.CreateSingleRequest( _Mp1, TestProcError );

			return CreateRequest( requestinfo );		
		}

		public override IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId)
		{
			throw new NotImplementedException();
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int umi)
		{
			throw new NotImplementedException();
		}
	}
}