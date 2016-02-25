namespace EZBob.DatabaseLib {
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using EZBob.DatabaseLib.Model.Marketplaces;

	public interface IAnalysisDataParameterInfo {
		ITimePeriod TimePeriod { get; }
		IDatabaseValueType ValueType { get; }
		object Value { get; }
		string ParameterName { get; }
	} // interface IAnalysisDataParameterInfo

	public class AnalysisDataParameterInfo : IAnalysisDataParameterInfo {
		public AnalysisDataParameterInfo(ITimePeriod time, object value, AggregationFunction function) {
			TimePeriod = time;
			Value = value;
			FunctionName = function;
		} // constructor

		public ITimePeriod TimePeriod { set; get; }

		public IDatabaseValueType ValueType { set; get; }

		public object Value { set; get; }

		public string ParameterName { get { return FunctionName.ToString(); } }

		public AggregationFunction FunctionName { get; set; }
	} // class AnalysisDataParameterInfo
} // namespace