using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Products;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.ServiceCalls;
using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public enum AmazonProductRequestType
	{
		ByAsin,
		BySellerSku
	}

	public class AmazonProductsRequestInfoBySellerSku : AmazonRequestInfoBase
	{
		public string SellerSku { get; set; }

		public AmazonProductRequestType Type
		{
			get { return AmazonProductRequestType.BySellerSku; }
		}

		internal AmazonProductItemBase RequestData( AmazonServiceProducts data, ActionAccessType access, RequestsCounterData requestCounte )
		{
			return data.GetProductCategoriesBySellerSku( this, access, requestCounte );
		}
	}
}