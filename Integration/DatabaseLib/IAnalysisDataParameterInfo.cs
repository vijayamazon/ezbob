using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	using EZBob.DatabaseLib.Model.Marketplaces;

	public interface IAnalysisDataParameterInfo
	{
		ITimePeriod TimePeriod { get; }
		IDatabaseValueType ValueType { get; }
		object Value { get; }
		string ParameterName { get; }
	}

	public class AnalysisDataParameterInfo : IAnalysisDataParameterInfo {
		public AnalysisDataParameterInfo(ITimePeriod time, object value, AggregationFunction function) {
			TimePeriod = time;
			Value = value;
			FunctionName = function;
		}

		public ITimePeriod TimePeriod { set; get; }

		public IDatabaseValueType ValueType { set; get; }

		public object Value { set; get; }

		public string ParameterName {
			get {
				return FunctionName.ToString();
			}
		}

		public AggregationFunction FunctionName { get; set; }
	}
}