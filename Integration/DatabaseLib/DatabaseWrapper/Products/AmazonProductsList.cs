namespace EZBob.DatabaseLib.DatabaseWrapper.Products
{
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib.ReceivedDataListLogic;

	public class AmazonProductsList : ReceivedDataListBase<AmazonProductItemBase>
	{
		public AmazonProductsList(DateTime submittedDate) 
			: base(submittedDate)
		{
		}
	}

	public abstract class AmazonProductItemBase
	{
		protected AmazonProductItemBase()
		{
			Categories = new List<AmazonProductCategory>();
		}

		public abstract string Key { get; }

		public List<AmazonProductCategory> Categories { get; private set; }
	}

	public class AmazonProductCategory
	{
		public string CategoryId { get; set; }
		public string CategoryName { get; set; }
		public AmazonProductCategory Parent { get; set; }
	}

	public class AmazonProductItemByAsin : AmazonProductItemBase
	{
		public AmazonProductItemByAsin(string productAsin)
		{
			ProductASIN = productAsin;
		}

		public string ProductASIN { get; private set; }

		public override string Key
		{
			get { return ProductASIN; }
		}
	}

	public class AmazonProductItemBySellerSku : AmazonProductItemBase
	{
		public AmazonProductItemBySellerSku( string productSellerSku )
		{
			ProductSellerSku = productSellerSku;
		}

		public string ProductSellerSku { get; private set; }

		public override string Key
		{
			get { return ProductSellerSku; }
		}
	}
}
