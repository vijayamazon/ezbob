using System;
using System.Collections.Generic;
using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib.Common
{
	public abstract class AmazonRequestInfoBase
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string MerchantId { get; set; }
		public List<string> MarketplaceId { get; set; }
		public int CustomerId { get; set; }
		public ErrorRetryingInfo ErrorRetryingInfo { get; set; }
		public string MWSAuthToken { get; set; }
	}
}
