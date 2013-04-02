using System.Linq;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderGetSiteCategories : DataProviderTokenDependentBase
	{
		public DataProviderGetSiteCategories( DataProviderCreationInfo info )
			: base( info )
		{
		}

		public ResultInfoEbayCategories GetSiteCategories()
		{
			var req = new GetCategoriesRequestType//();
			{
				CategorySiteID = "3", // UK				
				DetailLevel =  new[] { DetailLevelCodeType.ReturnAll, },
				//LevelLimit = 1,
				//LevelLimitSpecified =  true
			};
			var rez = base.GetServiceData( Service.GetCategories, req );

			if(rez.CategoryArray != null && rez.CategoryCountSpecified && rez.CategoryCount > 0)
			{
				var items = rez.CategoryArray.Where( c => c.NumOfItemsSpecified );
			}
			return new ResultInfoEbayCategories( rez );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetCategories; }
		}
	}
}