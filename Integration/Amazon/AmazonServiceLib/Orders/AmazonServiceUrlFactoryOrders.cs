using System;
using System.Security.Policy;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Orders
{
	class AmazonServiceUrlFactoryOrders : AmazonServiceUrlFactoryBase
	{
		public override Url Create(AmazonServiceCountry country)
		{
			string url;

			switch (country)
			{
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.Canada:
					url = "https://mws.amazonservices.ca/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.Germany:
					url = "https://mws.amazonservices.de/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.France:
					url = "https://mws.amazonservices.fr/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.Italy:
					url = "https://mws.amazonservices.it/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp/Orders/2011-01-01";
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn/Orders/2011-01-01";
					break;

				default:
					throw new NotImplementedException();	
			}

			return new Url( url );			
		}
	}
}