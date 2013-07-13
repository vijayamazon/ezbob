namespace EZBob.DatabaseLib {
	using System.Collections.Generic;
	using DatabaseWrapper.Order;
	using Model.Marketplaces.FreeAgent;
	using Model.Marketplaces.Sage;

	#region class AOrderComparer

	public abstract class AOrderComparer<T> : EqualityComparer<T> where T : class {
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

	class EkmOrderComparer : AOrderComparer<EkmOrderItem> {
		public override bool AreEqual(EkmOrderItem a, EkmOrderItem b) {
			return a.OrderNumber == b.OrderNumber;
		} // AreEqual

		public override int HashCode(EkmOrderItem a) {
			return a.OrderNumber.GetHashCode();
		} // HashCode
	} // class EkmOrderComparer

	#endregion class EkmOrderComparer

	#region class FreeAgentInvoiceComparer

	public class FreeAgentInvoiceComparer : AOrderComparer<MP_FreeAgentInvoice>
	{
		public override bool AreEqual(MP_FreeAgentInvoice a, MP_FreeAgentInvoice b)
		{
			return a.url == b.url;
		} // AreEqual

		public override int HashCode(MP_FreeAgentInvoice a)
		{
			return a.url.GetHashCode();
		} // HashCode
	} // class FreeAgentInvoiceComparer

	#endregion class FreeAgentInvoiceComparer

	#region class SageInvoiceComparer

	public class SageInvoiceComparer : AOrderComparer<MP_SageInvoice>
	{
		public override bool AreEqual(MP_SageInvoice a, MP_SageInvoice b)
		{
			return a.SageId == b.SageId;
		} // AreEqual

		public override int HashCode(MP_SageInvoice a)
		{
			return a.SageId.GetHashCode();
		} // HashCode
	} // class SageInvoiceComparer

	#endregion class SageInvoiceComparer

	#region class FreeAgentExpenseComparer

	public class FreeAgentExpenseComparer : AOrderComparer<MP_FreeAgentExpense> {
		public override bool AreEqual(MP_FreeAgentExpense a, MP_FreeAgentExpense b) {
			return a.url == b.url;
		} // AreEqual

		public override int HashCode(MP_FreeAgentExpense a) {
			return a.url.GetHashCode();
		} // HashCode
	} // class FreeAgentExpenseComparer

	#endregion class FreeAgentExpenseComparer

	#region class ChannelGrabberOrderComparer

	class ChannelGrabberOrderComparer : AOrderComparer<ChannelGrabberOrderItem> {
		public override bool AreEqual(ChannelGrabberOrderItem a, ChannelGrabberOrderItem b) {
			return a.NativeOrderId == b.NativeOrderId;
		} // AreEqual

		public override int HashCode(ChannelGrabberOrderItem a) {
			return a.NativeOrderId.GetHashCode();
		} // HashCode
	} // class ChannelGrabberOrderComparer

	#endregion class ChannelGrabberOrderComparer

	#region class PayPointOrderComparer

	class PayPointOrderComparer : AOrderComparer<PayPointOrderItem> {
		public override bool AreEqual(PayPointOrderItem a, PayPointOrderItem b) {
			return (a.trans_id == b.trans_id) && (a.date == b.date);
		} // AreEqual

		public override int HashCode(PayPointOrderItem a) {
			return a.trans_id.GetHashCode() ^ a.date.GetHashCode();
		} // HashCode
	} // class PayPointOrderComparer

	#endregion class PayPointOrderComparer
} // namespace EZBob.DatabaseLib
