using System;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.FunctionValues
{
	public interface IDatabaseAnalysisFunctionValues
	{	
		IDatabaseFunction Function { get; }
		object Value { get; }
		ITimePeriod TimePeriod { get; }
		DatabaseValueTypeEnum ValueType { get; }
		DateTime UpdatedDate { get; }
		int CountMonthsFor { get; }
	}
}