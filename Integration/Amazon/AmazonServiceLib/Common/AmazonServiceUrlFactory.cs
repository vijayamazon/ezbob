namespace EzBob.AmazonServiceLib.Common
{
	using System;
	using System.Security.Policy;
	using MarketWebService;
	using Orders;
	using Products;

	internal class AmazonServiceUrlHelper
	{
		public static IAmazonServiceUrlFactory CreateFactory( AmazonApiType type )
		{
			switch (type)
			{
				case AmazonApiType.Orders:
					return new AmazonServiceUrlFactoryOrders();

				case AmazonApiType.WebService:
					return new AmazonWebServiceUrlFactory();

				case AmazonApiType.Products:
					return new AmazonServiceUrlFactoryProducts();

				default:
					throw new NotImplementedException();
			}
		}
	}

	internal interface IAmazonServiceUrlFactory
	{
		Url Create( AmazonServiceCountry country );
	}

	abstract class AmazonServiceUrlFactoryBase : IAmazonServiceUrlFactory
	{
		public abstract Url Create( AmazonServiceCountry country );
	}
}