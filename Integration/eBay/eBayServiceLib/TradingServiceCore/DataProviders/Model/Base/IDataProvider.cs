using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public interface IDataProvider
	{
		ServiceVersion ApiVersion { get; }
		CallProcedureType CallProcedureType { get; }
		string CallProcedureName { get; }
		string CallProcedureDisplayName { get; }
		string CallProcedureDescription { get; }

		bool IsTokenDependent { get; }
	}
}