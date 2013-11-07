using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Orders.Model
{
	using System.Text;

	public class AmazonOrdersRequestInfo : AmazonRequestInfoBase
	{
		public string GetMarketPlacesString()
		{
			var sb = new StringBuilder();
			foreach (var mp in MarketplaceId)
			{
				sb.Append(mp);
				sb.Append(" ");
			}
			return sb.ToString();
		}
	}

	public class AmazonOrdersItemsRequestInfo : AmazonRequestInfoBase
	{
		public string OrderId { get; set; }
	}
}