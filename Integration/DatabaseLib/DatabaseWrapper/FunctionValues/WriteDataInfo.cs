using System;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.FunctionValues
{
	public interface IWriteDataInfo<out TEnum>
	{
		object Value { get; }
		TimePeriodEnum TimePeriodType { get; }
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

		public TEnum FunctionType { get; set; }
        public object Value { get; set; }
		public DateTime UpdatedDate { get; set; }
    	public TimePeriodEnum TimePeriodType { get; set; }

		public override string ToString()
		{
			return string.Format("{0} = {1} ({2})", FunctionType, Value, TimePeriodType);
		}
    }
}