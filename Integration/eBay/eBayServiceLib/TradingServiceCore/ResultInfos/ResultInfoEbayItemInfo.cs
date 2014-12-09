using EZBob.DatabaseLib;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayItemInfo : ResultInfoByServerResponseBase
	{
		private readonly GetItemResponseType _Response;

		public ResultInfoEbayItemInfo( GetItemResponseType response )
			: base(response)
		{
			_Response = response;

		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.ItemsInfo; }
		}

		public string ItemID
		{
			get { return _Response == null ? null : _Response.Item.ItemID; }
		}

		public eBayCategoryInfo PrimaryCategory
		{
			get { return _Response == null ? null : ConvertCategory( _Response.Item.PrimaryCategory ); }
		}

		public eBayCategoryInfo SecondaryCategory
		{
			get { return _Response == null? null : ConvertCategory( _Response.Item.SecondaryCategory ); }
		}

		public eBayCategoryInfo FreeAddedCategory
		{
			get { return _Response == null ? null : ConvertCategory( _Response.Item.FreeAddedCategory ); }
		}

		public string Title
		{
			get { return _Response == null ? null : _Response.Item.Title; }
		}

		private eBayCategoryInfo ConvertCategory( CategoryType data )
		{
			if ( data == null )
			{
				return null;
			}

			return new eBayCategoryInfo
			{
				CategoryId = data.CategoryID,
				//Level = data.CategoryLevelSpecified ? data.CategoryLevel : (int?)null,
				Name = data.CategoryName,
				IsVirtual = data.VirtualSpecified ? data.Virtual : (bool?)null,
				//ParentIdList = data.CategoryParentID
				//ParentNameList
			};
		}

	}
}
