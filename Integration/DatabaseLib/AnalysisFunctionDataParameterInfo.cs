using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public class AnalysisFunctionDataParameterInfo : AnalysisDataParameterInfoBase
	{
		private readonly object _Value;
		private readonly ITimePeriod _TimePeriod;
		private readonly IDatabaseFunction _Function;

		public AnalysisFunctionDataParameterInfo( IDatabaseFunction function, ITimePeriod timePeriod, object value)
		{
			_Function = function;
			_Value = value;
			_TimePeriod = timePeriod;
		}

		public override ITimePeriod TimePeriod
		{
			get { return _TimePeriod; }
		}

		public override IDatabaseValueType ValueType 
		{
			get { return _Function.FunctionValueType; }
		}

		public override object Value
		{
			get { return _Value; }
		}

		public override string ParameterName
		{
			get { return _Function == null ? null : _Function.DisplayName; }

		}
	}
}
