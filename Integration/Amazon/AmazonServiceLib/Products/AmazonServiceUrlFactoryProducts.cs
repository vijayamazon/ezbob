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
			const string apiVersion = "2011-10-01";

			switch ( country )
			{
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com/Products/" + apiVersion;
					break;
				case AmazonServiceCountry.Canada:
					url = "https://mws.amazonservices.ca/Products/" + apiVersion;
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp/Products/" + apiVersion;
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn/Products/" + apiVersion;
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk/Products/" + apiVersion;
					break;
				case AmazonServiceCountry.Germany:
				case AmazonServiceCountry.France:
				case AmazonServiceCountry.Italy:
					url = "https://mws-eu.amazonservices.com/Products/" + apiVersion;
					break;

				default:
					throw new NotImplementedException();
			}

			return new Url( url );
		}
	}
}
