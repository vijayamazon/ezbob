using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderUserInfo : DataProviderTokenDependentBase
	{
		public DataProviderUserInfo(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public static ResultInfoEbayUser GetDataAboutMySelf( DataProviderCreationInfo info )
		{
			return new DataProviderUserInfo( info ).GetDataAboutMySelf();
		}

		public ResultInfoEbayUser GetDataAboutMySelf()
		{
			return GetDataAbout( null );
		}

		public ResultInfoEbayUser GetDataAbout( string userId )
		{       
			var request = new GetUserRequestType 
			{ 
				UserID = userId,
				DetailLevel = new[]
				{
					DetailLevelCodeType.ReturnAll, 
				},			
				IncludeFeatureEligibility = true,
				IncludeFeatureEligibilitySpecified = true,				
			}; 
			GetUserResponseType response = base.GetServiceData( Service.GetUser, request );
			var rez = new ResultInfoEbayUser( response );
			rez.IncrementRequests( "GetUser" );
			return rez;
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetUser; }
		}
	}
}