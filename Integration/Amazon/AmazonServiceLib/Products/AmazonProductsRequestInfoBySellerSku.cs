namespace EzBob.AmazonServiceLib.Products
{
	using EZBob.DatabaseLib.DatabaseWrapper.Products;
	using Common;
	using ServiceCalls;
	using CommonLib;

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