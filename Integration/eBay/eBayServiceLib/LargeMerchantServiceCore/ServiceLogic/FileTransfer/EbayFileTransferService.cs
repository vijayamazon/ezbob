using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using EzBob.eBayServiceLib.FileTransferServiceReference;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.FileTransfer
{
	public class WebServiceFileTransferServicePort : FileTransferServicePortClient
	{
		private readonly LargeMerchantServiceWrapper _Wrapper;

		public WebServiceFileTransferServicePort(string serviceName, ServiceVersion version, IServiceTokenProvider tokenProvider, Binding binding, EndpointAddress  addr) 
			:base(binding, addr)			
		{
			_Wrapper = new LargeMerchantServiceWrapper(serviceName, version, tokenProvider);
		}

		public DownloadFileResponse DownloadFile( string jobId, string fileReferenceId )
		{
			using ( var scope = new OperationContextScope( this.InnerChannel ) )
			{
				var httpRequest = _Wrapper.CreateRequestMessageProperty( "downloadFile" );
				OperationContext.Current.OutgoingMessageProperties.Add( HttpRequestMessageProperty.Name, httpRequest );

				var downloadFileRequest = new DownloadFileRequest();
				downloadFileRequest.fileReferenceId = fileReferenceId;
				downloadFileRequest.taskReferenceId = jobId;

				var rez =  base.downloadFile( downloadFileRequest );
				return rez;
			}
		}
	}	
}