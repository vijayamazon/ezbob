using System.Diagnostics;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.FileTransferServiceReference;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.BulkData;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.FileTransfer;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public abstract class LargeMerchantServiceRequestBase : ILargeMerchantServiceRequestBase
	{
		// http://developer.ebay.com/DevZone/bulk-data-exchange/ReleaseNotes.html
		private readonly string _ApiVersion = "1.0.0";

		protected LargeMerchantServiceRequestBase( EbayBulkDataServiceProvider bulkServiceProvider, EbayFileTransferServiceProvider fileTransferProvider, IServiceTokenProvider tokenProvider )
		{
			BulkDataService = bulkServiceProvider.GetService(ReportType.ToString(), ApiVersion, tokenProvider);
			FileTransferService = fileTransferProvider.GetService( ReportType.ToString(), ApiVersion, tokenProvider );
		}

		protected WebServiceFileTransferServicePort FileTransferService { get; private set; }

		protected BulkDataExchangeServicePort BulkDataService { get; private set; }

		public abstract BulkDataReportType ReportType { get; }

		public ServiceVersion ApiVersion
		{
			get { return new ServiceVersion( _ApiVersion ); }
		}

		protected ResultDownloadFileRequest InternalGetReport(RequestsCounterData requestsCounter)
		{
			//var jobs = BulkDataService.GetJobs();

			//BulkDataService.DeleteRecurringJob( ReportType );

			ResultInfoJobRequest jobRez = BulkDataService.DownloadJob( ReportType );
			var reportTypeString = ReportType.ToString();

			requestsCounter.IncrementRequests( "DownloadJob", reportTypeString );
			if ( jobRez.HasError )
			{
				return new ResultDownloadFileRequest( jobRez.Errors, jobRez.SubmittedDate );
			}

			var fileReferenceId = jobRez.FileReferenceId;
			var jobId = jobRez.JobId;

			if ( string.IsNullOrEmpty( fileReferenceId ) )
			{
				JobProfile job = BulkDataService.GetJob( jobId );
				requestsCounter.IncrementRequests( "GetJob", reportTypeString );
				fileReferenceId = job.fileReferenceId;	

			}
			var rez = FileTransferService.DownloadFile( jobId, fileReferenceId );
			requestsCounter.IncrementRequests( "DownloadFile", reportTypeString );
			return new ResultDownloadFileRequest( rez );
			
		}
	}
}