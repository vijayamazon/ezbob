using System;
using System.Linq;
using System.Net;
using System.Threading;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.BulkData
{
	public class BulkDataExchangeServicePort : BulkDataExchangeService
	{
		private readonly LargeMerchantServiceWrapper _Wrapper;
		private string _CurrentOperationName;

		public BulkDataExchangeServicePort( string serviceName, ServiceVersion version, IServiceTokenProvider tokenProvider )
		{
			_Wrapper = new LargeMerchantServiceWrapper( serviceName, version, tokenProvider );
		}

		protected override WebRequest GetWebRequest( Uri uri )
		{
			var request = (HttpWebRequest)base.GetWebRequest( uri );

			return _Wrapper.CorrectRequest( _CurrentOperationName, request );
		}

		private StartDownloadJobResponse StartDownloadJob( BulkDataReportType reportType )
		{
			_CurrentOperationName = "startDownloadJob";

			var jobRequest = new StartDownloadJobRequest();
			jobRequest.downloadJobType = reportType.ToString();
			jobRequest.UUID = Guid.NewGuid().ToString();

			return base.startDownloadJob( jobRequest );
		}

		private GetJobStatusResponse GetJobStatus(string jobId)
		{
			var jobStatusRequest = new GetJobStatusRequest();
			jobStatusRequest.jobId = jobId;

			_CurrentOperationName = "getJobStatus";

			return base.getJobStatus( jobStatusRequest );
		}

		public ResultInfoJobRequest DownloadJob(BulkDataReportType reportType)
		{
			var jobs = GetJobs();

			//DeleteRecurringJob( reportType );
			//AbortIncompleteJob( reportType );

			StartDownloadJobResponse startDownloadJobResponse = StartDownloadJob( reportType );
			var rez = new ResultInfoDownloadJobRequest( startDownloadJobResponse );

			if ( rez.HasError )
			{
				return new ResultInfoJobRequest( rez.Errors, rez.SubmittedDate );
			}

			ResultInfoJobRequest jobRez = null;
			bool inProgress = true;

			while ( inProgress )
			{
				jobRez = new ResultInfoJobRequest( rez.JobId, GetJobStatus( rez.JobId ) );
				inProgress = jobRez.InProgress;

				Thread.Sleep( 1000 );
			}

			return jobRez;
		}

		public void DeleteRecurringJob(BulkDataReportType reportType)
		{
			var req = new GetRecurringJobsRequest();

			_CurrentOperationName = "getRecurringJobs";
			var r = base.getRecurringJobs( req );
			if ( r.recurringJobDetail == null )
			{
				return;
			}
			foreach ( RecurringJobDetail recurringJobDetail in r.recurringJobDetail )
			{
				if ( !recurringJobDetail.downloadJobType.Equals( reportType.ToString() ) )
				{
					continue;
				}
				_CurrentOperationName = "deleteRecurringJob";
				var deleteRecurringJobRequest = new DeleteRecurringJobRequest();
				deleteRecurringJobRequest.recurringJobId = recurringJobDetail.recurringJobId;
				var rez = base.deleteRecurringJob( deleteRecurringJobRequest );
			}

		}

		public void AbortJob( string jobId )
		{
			_CurrentOperationName = "getJobs";

			var req = new AbortJobRequest 
				{
					jobId = jobId
				};
			base.abortJob( req );
		}

		public void AbortIncompleteJob( BulkDataReportType reportType )
		{
			var jobs = GetJobs();

			jobs.jobProfile.Where( j => j.jobStatus != JobStatus.Completed && j.jobType.Equals( reportType.ToString() ) ).ToList().ForEach( j => AbortJob( j.jobId ) );

		}

		public GetJobsResponse GetJobs()
		{
			_CurrentOperationName = "getJobs";
			GetJobsRequest req = new GetJobsRequest();

			return base.getJobs( req );
		}

		public JobProfile GetJob(string jobId)
		{
			var jobs = GetJobs();

			return jobs.jobProfile.FirstOrDefault( j => j.jobId.Equals( jobId ) );
		}
	}
}
