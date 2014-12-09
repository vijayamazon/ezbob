using System;
// using EZBob.DatabaseLib.DatabaseWrapper.ValueType;

namespace Integration.ChannelGrabberConfig {

	public enum FunctionType {
		NumOfOrders,
		TotalSumOfOrders,
		AverageSumOfOrders,
		NumOfExpenses,
		TotalSumOfExpenses,
		AverageSumOfExpenses,
		TotalSumOfOrdersAnnualized
	} // enum FunctionType

	public enum ValueType {
		String,
		Integer,
		Double,
		Xml,
		DateTime,
		Boolean
	} // enum ValueType

	public class AggregatorInfo : ICloneable {

		public AggregatorInfo() {
			FunctionTypeName = "";
			ValueTypeName = "";
			ID = "";
		} // constructor

		public string FunctionTypeName { get; set; }
		public string ValueTypeName { get; set; }
		public string ID { get; set; }

		public void Parse() {
			if (!ChannelGrabberConfig.FunctionType.TryParse(FunctionTypeName, out m_nFunctionType))
				throw new ConfigException("Failed to parse function type.");

			if (!ChannelGrabberConfig.ValueType.TryParse(ValueTypeName, out m_nValueType))
				throw new ConfigException("Failed to parse value type.");

			try {
				m_oGuid = new Guid(ID);
			}
			catch (Exception e) {
				throw new ConfigException("Failed to parse ID: " + e.Message);
			}
		} // Parse

		public FunctionType FunctionType() { return m_nFunctionType; }
		public ValueType ValueType() { return m_nValueType; }
		public Guid Guid() { return m_oGuid; }

		public override string ToString() {
			return string.Format("( {0} - {1} - {2} )", FunctionType(), ValueType(), Guid());
		} // ToString

		public object Clone() {
			return new AggregatorInfo {
				FunctionTypeName = this.FunctionTypeName,
				ValueTypeName = this.ValueTypeName,
				ID = this.ID,
				m_nFunctionType = this.m_nFunctionType,
				m_nValueType = this.m_nValueType,
				m_oGuid = new Guid(m_oGuid.ToString())
			};
		} // Clone

		private FunctionType m_nFunctionType;
		private ValueType m_nValueType;
		private Guid m_oGuid;

	} // class AggregatorInfo

} // namespace Integration.ChannelGrabberConfig
