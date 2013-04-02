namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data
{
	public class CallProcedureInfo
	{
		public string ServiceName { get; private set; }

		public string Description { get; private set; }

		public string DisplayName { get; private set; }

		public CallProcedureInfo(string serviceName, string description, string displayName)
		{
			ServiceName = serviceName;
			Description = description;
			DisplayName = displayName;
		}
	}
}