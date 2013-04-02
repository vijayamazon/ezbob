namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public abstract  class DataInfo<T> : IParamsDataInfo
	{
		public T Value { get; set; }

		public DataInfo(T value)
		{
			Value = value;
		}

		public abstract bool HasData { get; }

		public override string ToString()
		{
			return Value.ToString();
		}

		public abstract DataInfoTypeEnum DataInfoType { get; }

	}
}