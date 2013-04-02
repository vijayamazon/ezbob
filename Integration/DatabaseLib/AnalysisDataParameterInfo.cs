using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public class AnalysisDataParameterInfo : AnalysisDataParameterInfoBase
	{
		private readonly ITimePeriod _TimePeriod;
		private readonly IDatabaseValueType _ValueType;
		private readonly object _Value;
		private readonly string _ParameterName;

		public AnalysisDataParameterInfo(string parameterName, ITimePeriod timePeriod, IDatabaseValueType valueType, object value)
		{
			_TimePeriod = timePeriod;
			_ValueType = valueType;
			_Value = value;
			_ParameterName = parameterName;
		}

		public override ITimePeriod TimePeriod
		{
			get { return _TimePeriod; }			
		}
		
		public override IDatabaseValueType ValueType
		{
			get { return _ValueType; }			
		}
		public override object Value
		{
			get { return _Value; }			
		}
		
		public override string ParameterName
		{
			get { return _ParameterName; }			
		}

		public override int CountMonths
		{
			get { return 0; }
		}
	}
}