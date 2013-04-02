using System;
using System.Security.Policy;
using EzBob.AmazonServiceLib.Inventory;
using EzBob.AmazonServiceLib.MarketWebService;
using EzBob.AmazonServiceLib.Orders;
using EzBob.AmazonServiceLib.Products;

namespace EzBob.AmazonServiceLib.Common
{
	internal class AmazonServiceUrlHelper
	{
		public static IAmazonServiceUrlFactory CreateFactory( AmazonApiType type )
		{
			switch (type)
			{
				case AmazonApiType.Orders:
					return new AmazonServiceUrlFactoryOrders();

				case AmazonApiType.Inventory:
					return new AmazonServiceUrlFactoryInventory();

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