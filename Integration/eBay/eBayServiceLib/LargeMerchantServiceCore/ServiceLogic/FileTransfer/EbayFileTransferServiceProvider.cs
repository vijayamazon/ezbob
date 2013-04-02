
using System;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.FileTransfer
{
	public class EbayFileTransferServiceProvider : EbayLargeMerchantServicesProviderBase<WebServiceFileTransferServicePort>
	{
		public EbayFileTransferServiceProvider( EbayServiceConnectionInfo dataInfo )
			: base( dataInfo )
		{
		}

		public override WebServiceFileTransferServicePort GetService( string callProcedureName, ServiceVersion ver, IServiceTokenProvider tokenProvider )
		{
			var transportBindingElement = new HttpsTransportBindingElement();
			transportBindingElement.MaxReceivedMessageSize = 2147483647;
			transportBindingElement.MaxBufferSize = 2147483647;

			var customBinding = new CustomBinding( new BindingElement[]
					{
						new MtomMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8),
						transportBindingElement
					}
				);
			
			customBinding.OpenTimeout = new TimeSpan( 0, 5, 0 );
			customBinding.ReceiveTimeout = new TimeSpan( 0, 20, 0 );
			customBinding.SendTimeout = new TimeSpan( 0, 5, 0 );
			var addr = new EndpointAddress( Endpoint );
			var service = new WebServiceFileTransferServicePort( ServiceType.ToString(), ver, tokenProvider, customBinding, addr );
			
			return service;
		}

		public override EbayServiceType ServiceType
		{
			get { return EbayServiceType.FileTransferService; }
		}
	}	
}