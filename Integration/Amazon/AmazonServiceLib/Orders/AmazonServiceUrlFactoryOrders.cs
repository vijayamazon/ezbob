using System;
using System.Security.Policy;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Orders {
	class AmazonServiceUrlFactoryOrders : AmazonServiceUrlFactoryBase
	{
		public override Url Create(AmazonServiceCountry country)
		{
			string url;
			const string apiVersion = "2013-09-01";
			switch (country) {
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.Canada:
					url = "https://mws.amazonservices.ca/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.Germany:
					url = "https://mws.amazonservices.de/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.France:
					url = "https://mws.amazonservices.fr/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.Italy:
					url = "https://mws.amazonservices.it/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp/Orders/" + apiVersion;
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn/Orders/" + apiVersion;
					break;

				default:
					throw new NotImplementedException();
			}

			return new Url(url);
		}
	}
}