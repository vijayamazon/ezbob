using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Products
{
	class AmazonServiceUrlFactoryProducts : AmazonServiceUrlFactoryBase
	{
		public override Url Create( AmazonServiceCountry country )
		{
			string url;

			switch ( country )
			{
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com/Products/2011-10-01";
					break;
				case AmazonServiceCountry.Canada:
					url = "https://mws.amazonservices.ca/Products/2011-10-01";
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp/Products/2011-10-01";
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn/Products/2011-10-01";
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk/Products/2011-10-01";
					break;
				case AmazonServiceCountry.Germany:
				case AmazonServiceCountry.France:
				case AmazonServiceCountry.Italy:
					url = "https://mws-eu.amazonservices.com/Products/2011-10-01";
					break;

				

				default:
					throw new NotImplementedException();
			}

			return new Url( url );
		}
	}
}
