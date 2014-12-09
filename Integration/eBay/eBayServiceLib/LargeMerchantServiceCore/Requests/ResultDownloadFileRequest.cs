using System;
using System.Collections.Generic;
using EzBob.eBayServiceLib.FileTransferServiceReference;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public class ResultDownloadFileRequest : ResultInfoByFiletransferServiveResponseBase
	{
		private readonly DownloadFileResponse _Response;

		public ResultDownloadFileRequest( DownloadFileResponse response ) 
			: base(response)
		{
			_Response = response;
		}

		public ResultDownloadFileRequest( IEnumerable<ErrorInfo> errors, DateTime submittedDate ) 
			: base(errors, submittedDate)
		{
		}

		public FileAttachment FileAttachment 
		{
			get { return _Response.fileAttachment; }
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.DownloadedFileInfo; }
		}

	}
}
