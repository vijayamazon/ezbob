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

	public abstract class AmazonProductsRequestBase : AmazonRequestInfoBase
	{
		public abstract AmazonProductRequestType Type { get; }

		internal abstract AmazonProductItemBase RequestData( AmazonServiceProducts data, ActionAccessType access, RequestsCounterData requestCounte );
	}

	public class AmazonProductsRequestInfoByAsin : AmazonProductsRequestBase
	{
		public string ProductASIN { get; set; }

		public override AmazonProductRequestType Type
		{
			get { return AmazonProductRequestType.ByAsin; }
		}

		internal override AmazonProductItemBase RequestData( AmazonServiceProducts data, ActionAccessType access, RequestsCounterData requestCounte )
		{
			return data.GetProductCategoriesByAsin( this, access, requestCounte );
		}
	}

	public class AmazonProductsRequestInfoBySellerSku : AmazonProductsRequestBase
	{
		public string SellerSku { get; set; }

		public override AmazonProductRequestType Type
		{
			get { return AmazonProductRequestType.BySellerSku; }
		}

		internal override AmazonProductItemBase RequestData( AmazonServiceProducts data, ActionAccessType access, RequestsCounterData requestCounte )
		{
			return data.GetProductCategoriesBySellerSku( this, access, requestCounte );
		}
	}
}