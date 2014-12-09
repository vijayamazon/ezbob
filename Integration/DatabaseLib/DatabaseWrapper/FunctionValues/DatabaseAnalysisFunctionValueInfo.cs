using System;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.FunctionValues
{
	class DatabaseAnalysisFunctionValueInfo : IDatabaseAnalysisFunctionValues
	{
		public DatabaseAnalysisFunctionValueInfo( IDatabaseFunction function, ITimePeriod timePeriod, object value, DateTime updatedDate )
		{			
			Function = function;
			TimePeriod = timePeriod;
			Value = value;
			UpdatedDate = updatedDate;
		}

		public IDatabaseFunction Function { get; private set; }
		public ITimePeriod TimePeriod { get; private set; }
		public DateTime UpdatedDate { get; private set; }

		public object Value { get; private set; }

		public DatabaseValueTypeEnum ValueType
		{
			get { return Function.FunctionValueType.ValueType; }
		}
	}
}
