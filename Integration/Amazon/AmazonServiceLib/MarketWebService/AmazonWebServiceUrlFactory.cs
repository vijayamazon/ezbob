using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.MarketWebService
{
	class AmazonWebServiceUrlFactory : AmazonServiceUrlFactoryBase
	{
		public override Url Create( AmazonServiceCountry country )
		{
			string url;

			switch ( country )
			{
				case AmazonServiceCountry.US:
					url = "https://mws.amazonservices.com";
					break;

				case AmazonServiceCountry.UK:
					url = "https://mws.amazonservices.co.uk";
					break;

				case AmazonServiceCountry.Germany:
					url = "https://mws.amazonservices.de";
					break;

				case AmazonServiceCountry.France:
					url = "https://mws.amazonservices.fr";
					break;

				case AmazonServiceCountry.Japan:
					url = "https://mws.amazonservices.jp";
					break;

				case AmazonServiceCountry.China:
					url = "https://mws.amazonservices.com.cn";
					break;

				case AmazonServiceCountry.Canada:
					url = "https://mws.amazonservices.ca";
					break;

				case AmazonServiceCountry.Italy:
					url = "https://mws.amazonservices.it";
					break;

				default:
					throw new NotImplementedException();
			}

			return new Url( url );
		}
	}
}
