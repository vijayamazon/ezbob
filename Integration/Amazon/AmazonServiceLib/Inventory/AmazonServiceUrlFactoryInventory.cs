using System;
using System.Security.Policy;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Inventory
{
	class AmazonServiceUrlFactoryInventory : AmazonServiceUrlFactoryBase
	{
		public override Url Create( AmazonServiceCountry country )
		{
			string url;

			switch ( country )
			{
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com/FulfillmentInventory/2010-10-01/";
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk/FulfillmentInventory/2010-10-01/";
					break;

				case AmazonServiceCountry.Germany:
					url = "https://mws.amazonservices.de/FulfillmentInventory/2010-10-01/";
					break;

				case AmazonServiceCountry.France:
					url = "https://mws.amazonservices.fr/FulfillmentInventory/2010-10-01/";
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp/FulfillmentInventory/2010-10-01/";
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn/FulfillmentInventory/2010-10-01/";
					break;

				default:
					throw new NotImplementedException();
			}

			return new Url( url );
		}
	}
}