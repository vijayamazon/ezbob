using EZBob.DatabaseLib;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderGetItemInfo : DataProviderTokenDependentBase
	{
		public DataProviderGetItemInfo( DataProviderCreationInfo info )
			: base( info )
		{
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetItem; }
		}

		public ResultInfoEbayItemInfo GetItem(eBayRequestItemInfoData requestData)
		{
			var req = new GetItemRequestType
			{
				ItemID = requestData.ItemId
			};
			var response = base.GetServiceData( Service.GetItem, req );
			var rez = new ResultInfoEbayItemInfo( response );
			rez.IncrementRequests( "GetItem", req.ItemID );
			return rez;
		}
	}

	public class eBayRequestItemInfoData
	{
		public eBayRequestItemInfoData(eBayFindOrderItemInfoData data)
		{
			ItemId = data.ItemId;
		}

		public eBayRequestItemInfoData(string itemId)
		{
			ItemId = itemId;
		}

		public string ItemId { get; private set; }
	}
}