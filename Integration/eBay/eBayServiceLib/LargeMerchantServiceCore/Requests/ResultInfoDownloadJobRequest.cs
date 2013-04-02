using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public class ResultInfoDownloadJobRequest : ResultInfoByBulkDataServiveResponseBase
	{
		private readonly StartDownloadJobResponse _ServiceResponse;

		public ResultInfoDownloadJobRequest( StartDownloadJobResponse serviceResponse ) 
			: base(serviceResponse)
		{
			_ServiceResponse = serviceResponse;
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.JobStatusRequestInfo; }
		}

		public string JobId
		{
			get { return _ServiceResponse.jobId; }
		}		
	}
}