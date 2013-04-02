using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public abstract class AnalysisDataParameterInfoBase : IAnalysisDataParameterInfo
	{
		public abstract ITimePeriod TimePeriod { get; }
		public abstract IDatabaseValueType ValueType { get; }
		public abstract object Value { get; }
		public abstract string ParameterName { get; }

		public abstract int CountMonths { get; }

		public override string ToString()
		{
			return string.Format("{0} - {1}: {2} ({3})", ParameterName, TimePeriod, Value, ValueType);
		}
	}
}