using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Orders.Model
{
	public class AmazonOrdersRequestInfo : AmazonRequestInfoBase
	{
		
	}

	public class AmazonOrdersItemsRequestInfo : AmazonRequestInfoBase
	{
		public string OrderId { get; set; }
	}
}