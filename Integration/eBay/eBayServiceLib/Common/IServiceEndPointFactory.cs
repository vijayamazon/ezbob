using System;
using System.Security.Policy;

namespace EzBob.eBayServiceLib.Common
{
	public enum ServiceEndPointType { Sandbox, Production }

	public interface IServiceEndPointFactory
	{
		Url Create( EbayServiceType serviceType, ServiceEndPointType endPointType );
	}

	public class ServiceEndPointFactory : IServiceEndPointFactory
	{
		public Url Create( EbayServiceType serviceType, ServiceEndPointType endPointType )
		{
			switch ( serviceType )
			{
				case EbayServiceType.Trading:
					{
						switch (endPointType)
						{
							case ServiceEndPointType.Sandbox:
								return new Url("https://api.sandbox.ebay.com/wsapi");

							case ServiceEndPointType.Production:
								return new Url("https://api.ebay.com/wsapi");

							default:
								throw new NotImplementedException();
						}
					}

				case EbayServiceType.BulkDataExchangeService:
					{
						switch ( endPointType )
						{
							case ServiceEndPointType.Sandbox:
								return new Url( "https://webservices.sandbox.ebay.com/BulkDataExchangeService" );

							case ServiceEndPointType.Production:
								return new Url( "https://webservices.ebay.com/BulkDataExchangeService" );

							default:
								throw new NotImplementedException();
						}
					}

				case EbayServiceType.FileTransferService:
					{
						switch ( endPointType )
						{
							case ServiceEndPointType.Sandbox:
								return new Url( "https://storage.sandbox.ebay.com/FileTransferService" );

							case ServiceEndPointType.Production:
								return new Url( "https://storage.ebay.com/FileTransferService" );

							default:
								throw new NotImplementedException();
						}
					}

				default:
					throw new NotImplementedException();
			}
		}
	}
}