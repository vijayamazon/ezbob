using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public interface IAnalysisDataParameterInfo
	{
		ITimePeriod TimePeriod { get; }
		IDatabaseValueType ValueType { get; }
		object Value { get; }
		string ParameterName { get; }
		int CountMonths { get; }
	}
}