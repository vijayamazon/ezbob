namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data
{
	public abstract class CallProcedureType
	{
		protected CallProcedureType(CallProcedureTypeEnum type)
		{
			Type = type;
		}

		public CallProcedureTypeEnum Type { get; private set; }

		public abstract bool IsTokenDependent { get; }
	}
}