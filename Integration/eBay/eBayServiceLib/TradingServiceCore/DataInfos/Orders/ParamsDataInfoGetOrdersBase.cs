namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders
{
	public abstract class ParamsDataInfoGetOrdersBase : IParamsDataInfo
	{
		public DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.GetOrdersParams; }
		}

		public abstract bool HasData { get; }

		public abstract ParamsDataInfoGetOrdersParamsType Type { get; }
	}
}