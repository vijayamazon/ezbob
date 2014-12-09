namespace EZBob.DatabaseLib {
	using System.Collections.Generic;
	using DatabaseWrapper.Order;
	using Model.Marketplaces.Amazon;
	using Model.Marketplaces.FreeAgent;
	using Model.Marketplaces.Sage;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

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

	class EkmOrderComparer : AOrderComparer<EkmOrderItem> {
		public override bool AreEqual(EkmOrderItem a, EkmOrderItem b) {
			return a.OrderNumber == b.OrderNumber;
		} // AreEqual

		public override int HashCode(EkmOrderItem a) {
			return a.OrderNumber.GetHashCode();
		} // HashCode
	} // class EkmOrderComparer

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

	public class SageSalesInvoiceComparer : AOrderComparer<MP_SageSalesInvoice>
	{
		public override bool AreEqual(MP_SageSalesInvoice a, MP_SageSalesInvoice b)
		{
			return a.SageId == b.SageId;
		} // AreEqual

		public override int HashCode(MP_SageSalesInvoice a)
		{
			return a.SageId.GetHashCode();
		} // HashCode
	} // class SageSalesInvoiceComparer

	public class SagePurchaseInvoiceComparer : AOrderComparer<MP_SagePurchaseInvoice>
	{
		public override bool AreEqual(MP_SagePurchaseInvoice a, MP_SagePurchaseInvoice b)
		{
			return a.SageId == b.SageId;
		} // AreEqual

		public override int HashCode(MP_SagePurchaseInvoice a)
		{
			return a.SageId.GetHashCode();
		} // HashCode
	} // class SagePurchaseInvoiceComparer

	public class SageIncomeComparer : AOrderComparer<MP_SageIncome>
	{
		public override bool AreEqual(MP_SageIncome a, MP_SageIncome b)
		{
			return a.SageId == b.SageId;
		} // AreEqual

		public override int HashCode(MP_SageIncome a)
		{
			return a.SageId.GetHashCode();
		} // HashCode
	} // class SageIncomeComparer

	public class SageExpenditureComparer : AOrderComparer<MP_SageExpenditure>
	{
		public override bool AreEqual(MP_SageExpenditure a, MP_SageExpenditure b)
		{
			return a.SageId == b.SageId;
		} // AreEqual

		public override int HashCode(MP_SageExpenditure a)
		{
			return a.SageId.GetHashCode();
		} // HashCode
	} // class SageExpenditureComparer

	public class FreeAgentExpenseComparer : AOrderComparer<MP_FreeAgentExpense> {
		public override bool AreEqual(MP_FreeAgentExpense a, MP_FreeAgentExpense b) {
			return a.url == b.url;
		} // AreEqual

		public override int HashCode(MP_FreeAgentExpense a) {
			return a.url.GetHashCode();
		} // HashCode
	} // class FreeAgentExpenseComparer

	public class InternalOrderComparer : AOrderComparer<AInternalOrderItem> {
		public override bool AreEqual(AInternalOrderItem a, AInternalOrderItem b) {
			return a.NativeOrderId == b.NativeOrderId;
		} // AreEqual

		public override int HashCode(AInternalOrderItem a) {
			return a.NativeOrderId.GetHashCode();
		} // HashCode
	} // class InternalOrderComparer

	class PayPointOrderComparer : AOrderComparer<PayPointOrderItem> {
		public override bool AreEqual(PayPointOrderItem a, PayPointOrderItem b) {
			return (a.trans_id == b.trans_id) && (a.date == b.date);
		} // AreEqual

		public override int HashCode(PayPointOrderItem a) {
			return a.trans_id.GetHashCode() ^ a.date.GetHashCode();
		} // HashCode
	} // class PayPointOrderComparer

	class YodleeOrderComparer : AOrderComparer<BankTransactionData>
	{
		public override bool AreEqual(BankTransactionData a, BankTransactionData b)
		{
			if (string.IsNullOrEmpty(a.srcElementId) && string.IsNullOrEmpty(b.srcElementId)) {
				return false;
			}
			return (a.srcElementId == b.srcElementId);
		} // AreEqual

		public override int HashCode(BankTransactionData a)
		{
			if (string.IsNullOrEmpty(a.srcElementId)) {
				return a.bankTransactionId.GetHashCode();
			}
			return a.srcElementId.GetHashCode();
		} // HashCode
	} // class YodleeOrderComparer

	class YodleeTransactionComparer: AOrderComparer<MP_YodleeOrderItemBankTransaction>
	{
		public override bool AreEqual(MP_YodleeOrderItemBankTransaction a, MP_YodleeOrderItemBankTransaction b)
		{
			if (string.IsNullOrEmpty(a.srcElementId) && string.IsNullOrEmpty(b.srcElementId)) {
				return a.Id == b.Id;
			}

			return a.srcElementId == b.srcElementId;
		}

		public override int HashCode(MP_YodleeOrderItemBankTransaction a)
		{
			if (string.IsNullOrEmpty(a.srcElementId)) {
				return a.Id.GetHashCode();
			}

			return a.srcElementId.GetHashCode();
		}
	}

	class AmazonOrderComparer : AOrderComparer<MP_AmazonOrderItem> {
		public override bool AreEqual(MP_AmazonOrderItem a, MP_AmazonOrderItem b) {
			if (string.IsNullOrEmpty(a.OrderId) && string.IsNullOrEmpty(b.OrderId)) {
				return false;
			}
			return (a.OrderId == b.OrderId);
		} // AreEqual

		public override int HashCode(MP_AmazonOrderItem a) {
			return a.OrderId.GetHashCode();
		} // HashCode
	} // class AmazonOrderComparer

} // namespace EZBob.DatabaseLib
