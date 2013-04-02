using System;
using System.Collections.Generic;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap.bulkdataexchange;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public abstract class ResultInfoByBulkDataServiveResponseBase : EbayResultInfoBase
	{
		protected ResultInfoByBulkDataServiveResponseBase( BaseServiceResponse serviceResponse )
			:base(serviceResponse.timestamp)
		{
		}

		protected ResultInfoByBulkDataServiveResponseBase( IEnumerable<ErrorInfo> errors, DateTime submittedDate )
			: base( errors, submittedDate )
		{
		}
	}
}