using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	public interface ILargeMerchantServiceRequestBase
	{
		BulkDataReportType ReportType { get; }
		ServiceVersion ApiVersion { get; }
	}
}