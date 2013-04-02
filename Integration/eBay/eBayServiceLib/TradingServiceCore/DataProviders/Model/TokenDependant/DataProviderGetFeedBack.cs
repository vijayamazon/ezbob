using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public  class DataProviderGetFeedback : DataProviderTokenDependentBase
	{
		public DataProviderGetFeedback(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public static ResultInfoEbayFeedBack GetFeedBack( DataProviderCreationInfo info )
		{
			return new DataProviderGetFeedback( info ).GetFeedBack();
		}

		private ResultInfoEbayFeedBack  GetFeedBack()
		{
			var req = new GetFeedbackRequestType() ;
			var  response = base.GetServiceData(Service.GetFeedback , req );

		    var rez = new ResultInfoEbayFeedBack( response);
			rez.IncrementRequests( "GetFeedback" );
			return rez;
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetFeedback; }
		}
	}
}
