using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public abstract class AInternalOrderItem : TimeDependentRangedDataBase {
		public virtual string NativeOrderId { get; set; }
	} // class AInternalOrderItem
} // namespace