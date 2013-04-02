using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public class ResultInfoJobRequest : ResultInfoByBulkDataServiveResponseBase
	{
		private readonly GetJobStatusResponse _Response;

		public ResultInfoJobRequest( IEnumerable<ErrorInfo> errors, DateTime submittedDate )
			: base( errors, submittedDate )
		{
		}

		public ResultInfoJobRequest( string jobId, GetJobStatusResponse response ) 
			: base(response)
		{
			JobId = jobId;
			_Response = response;
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.JobRequestInfo; }
		}

		public bool IsDone
		{
			get
			{
				if ( _Response.jobProfile == null )
				{
					return false;
				}
				var job = _Response.jobProfile.FirstOrDefault( j => j.jobId.Equals( JobId ) );
				Debug.Assert( job != null );

				return job.jobStatus == JobStatus.Completed;
			}
		}

		public bool InProgress
		{
			get
			{
				if ( _Response.jobProfile == null )
				{
					return false;
				}
				var job = _Response.jobProfile.FirstOrDefault( j => j.jobId.Equals( JobId ) );
				Debug.Assert( job != null );

				return job.jobStatus == JobStatus.InProcess || job.jobStatus == JobStatus.Created || job.jobStatus == JobStatus.Scheduled;
			}
		}

		public string FileReferenceId
		{
			get 
			{
				if ( _Response.jobProfile == null )
				{
					return String.Empty;
				}
				var job = _Response.jobProfile.FirstOrDefault( j => j.jobId.Equals( JobId ) );
				Debug.Assert( job != null );

				return job.fileReferenceId;
			}
		}

		public string JobId { get; private set; }
	}
}