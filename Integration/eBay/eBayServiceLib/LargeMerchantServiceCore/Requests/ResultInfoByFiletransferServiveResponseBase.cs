using System;
using System.Collections.Generic;
using EzBob.eBayServiceLib.FileTransferServiceReference;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public abstract class ResultInfoByFiletransferServiveResponseBase : EbayResultInfoBase
	{
		private readonly BaseServiceResponse _ServiceResponse;

		protected ResultInfoByFiletransferServiveResponseBase( BaseServiceResponse serviceResponse )
			:base(serviceResponse.timestamp)
		{
			_ServiceResponse = serviceResponse;
		}

		protected ResultInfoByFiletransferServiveResponseBase( IEnumerable<ErrorInfo> errors, DateTime submittedDate )
			: base( errors, submittedDate )
		{
		}
	}
}