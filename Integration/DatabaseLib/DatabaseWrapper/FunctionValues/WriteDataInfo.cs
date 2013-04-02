using System;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.FunctionValues
{
	public interface IWriteDataInfo<out TEnum>
	{
		object Value { get; }
		TimePeriodEnum TimePeriodType { get; }
		int CountMonthsFor { get; }
		TEnum FunctionType { get; }
		DateTime UpdatedDate { get; }
	}

	public class WriteDataInfo<TEnum> : IWriteDataInfo<TEnum>
    {
        public WriteDataInfo()
        {
        }

        public WriteDataInfo( TEnum functionType, object value, DateTime updatedDate, TimePeriodEnum timePeriodType)
        {		
            FunctionType = functionType;
            Value = value;
        	UpdatedDate = updatedDate;
        	TimePeriodType = timePeriodType;
        }

		public int CountMonthsFor { get; set; }
		public TEnum FunctionType { get; set; }
        public object Value { get; set; }
		public DateTime UpdatedDate { get; set; }
    	public TimePeriodEnum TimePeriodType { get; set; }


		public override string ToString()
		{
			return string.Format("{0} = {1} ({2} = {3})", FunctionType, Value, TimePeriodType, CountMonthsFor);
		}
    }
}