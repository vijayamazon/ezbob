using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Exceptions;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonServiceLib.UserInfo;
using EzBob.CommonLib;
using EzBob.CommonLib.TrapForThrottlingLogic;
using EzBob.RequestsQueueCore;
using EzBob.RequestsQueueCore.RequestStates;
using StructureMap;

namespace EzBob
{
	public static class RetrieveDataHelper
	{
		private static readonly IEzBobIntegrationWorkflows _EzBobIntegrationWorkflows;

		static RetrieveDataHelper()
		{
			_EzBobIntegrationWorkflows = EzBobIntegrationWorkflows.Instance;
			//_EzBobIntegrationWorkflows = EzBobIntegrationWorkflowsMock.Instance;
		}

		/// <summary>
		/// First Retrieve Data for Customer's MarketPlace
		/// </summary>
		/// <param name="customerMarketPlaceId">customerMarketPlaceId - stored in DB</param>
		/// <returns>Request Handle - number</returns>
		public static int UpdateCustomerMarketplaceData( int customerMarketPlaceId )
		{
			return _EzBobIntegrationWorkflows.UpdateCustomerMarketPlaceData( customerMarketPlaceId );
		}

		/// <summary>
		/// Update data for All Customer's Market Places
		/// </summary>
		/// <param name="customerId">Customer Id - stored in DB</param>
		/// <returns>Request Handle - number</returns>
		public static int UpdateCustomerData( int customerId )
		{
			return _EzBobIntegrationWorkflows.UpdateCustomerData( customerId );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestNumber">Request Handle - number</param>
		/// <returns>true - then request finished, false - otherwise</returns>
		public static bool IsRequestDone( int requestNumber )
		{
			return _EzBobIntegrationWorkflows.IsRequestDone( requestNumber );
		}

		/// <summary>
		/// Get Request State
		/// </summary>
		/// <param name="requestNumber">Request Handle - number</param>
		/// <returns>Request State</returns>
		/// <see cref="EzBob.RequestsQueueCore.RequestStates"/>
		public static IRequestState GetRequestState( int requestNumber )
		{
			return _EzBobIntegrationWorkflows.GetRequestState( requestNumber );
		}

		/// <summary>
		/// Get Error for Request
		/// </summary>
		/// <param name="requestNumber">Request Handle - number</param>
		/// <returns>Error string</returns>
		public static string GetError( int requestNumber )
		{
			return _EzBobIntegrationWorkflows.GetError( requestNumber );
		}

		public static bool IsAmazonUserCorrect( AmazonUserInfo userInfo )
		{
			return AmazonRateInfo.IsUserCorrect( userInfo );
		}

		public static IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace( int umi )
		{
			return _EzBobIntegrationWorkflows.GetAnalysisValuesByCustomerMarketPlace( umi );
		}


		public static IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo( int umi )			
		{
			return _EzBobIntegrationWorkflows.RetrieveCustomerSecurityInfo( umi );
		}
		
		public static void Exit()
		{
			TrapForThrottlingController.Exit();
			_EzBobIntegrationWorkflows.Exit();
		}
	}
}
