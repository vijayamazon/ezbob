namespace EZBob.DatabaseLib {
	using System.Collections.Generic;
	using DatabaseWrapper.Order;

	#region class AOrderComparer

	public abstract class AOrderComparer<T> : EqualityComparer<T> where T : class
	{
		public override bool Equals(T a, T b) {
			if (object.ReferenceEquals(a, b))
				return true;

			if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
				return false;

			return AreEqual(a, b);
		} // Equals

		public override int GetHashCode(T a) {
			return object.ReferenceEquals(a, null) ? 0 : HashCode(a);
		} // GetHashCode

		public abstract bool AreEqual(T a, T b);
		public abstract int HashCode(T a);
	} // class AOrderComparer

	#endregion class AOrderComparer

	#region class EkmOrderComparer

	class EkmOrderComparer : AOrderComparer<EkmOrderItem>  {
		public override bool AreEqual(EkmOrderItem a, EkmOrderItem b) {
			return a.OrderNumber == b.OrderNumber;
		} // AreEqual

		public override int HashCode(EkmOrderItem a) {
			return a.OrderNumber.GetHashCode();
		} // HashCode
	} // class EkmOrderComparer

	#endregion class EkmOrderComparer

	public class FreeAgentInvoiceComparer : AOrderComparer<FreeAgentInvoice>
	{
		public override bool AreEqual(FreeAgentInvoice a, FreeAgentInvoice b)
		{
			return a.url == b.url;
		} // AreEqual

		public override int HashCode(FreeAgentInvoice a)
		{
			return a.url.GetHashCode();
		} // HashCode
	} // class FreeAgentInvoiceComparer

	public class FreeAgentExpenseComparer : AOrderComparer<FreeAgentExpense>
	{
		public override bool AreEqual(FreeAgentExpense a, FreeAgentExpense b)
		{
			return a.url == b.url;
		} // AreEqual

		public override int HashCode(FreeAgentExpense a)
		{
			return a.url.GetHashCode();
		} // HashCode
	} // class FreeAgentExpenseComparer

	#region class ChannelGrabberOrderComparer

	class ChannelGrabberOrderComparer<T> : AOrderComparer<T> where T: class, IChannelGrabberOrderItem {
		public override bool AreEqual(T a, T b) {
			return a.NativeOrderId == b.NativeOrderId;
		} // AreEqual

		public override int HashCode(T a) {
			return a.NativeOrderId.GetHashCode();
		} // HashCode
	} // class ChannelGrabberOrderComparer

	#endregion class ChannelGrabberOrderComparer

	class VolusionOrderComparer : ChannelGrabberOrderComparer<VolusionOrderItem> {}
	class PlayOrderComparer : ChannelGrabberOrderComparer<PlayOrderItem> {}

	#region class PayPointOrderComparer

	class PayPointOrderComparer : AOrderComparer<PayPointOrderItem>  {
		public override bool AreEqual(PayPointOrderItem a, PayPointOrderItem b) {
			return (a.trans_id == b.trans_id) && (a.date == b.date);
		} // AreEqual

		public override int HashCode(PayPointOrderItem a) {
			return a.trans_id.GetHashCode() ^ a.date.GetHashCode();
		} // HashCode
	} // class PayPointOrderComparer

	#endregion class PayPointOrderComparer

} // namespace EZBob.DatabaseLib
