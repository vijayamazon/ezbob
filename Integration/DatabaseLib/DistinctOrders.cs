﻿namespace EZBob.DatabaseLib {
	using System.Collections.Generic;
	using DatabaseWrapper.Order;
	using Model.Marketplaces.FreeAgent;
	using Model.Marketplaces.Sage;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

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

	#region class SageSalesInvoiceComparer

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

	#endregion class SageSalesInvoiceComparer

	#region class SagePurchaseInvoiceComparer

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

	#endregion class SagePurchaseInvoiceComparer

	#region class SageIncomeComparer

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

	#endregion class SageIncomeComparer

	#region class SageExpenditureComparer

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

	#endregion class SageExpenditureComparer

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

	#region class InternalOrderComparer

	public class InternalOrderComparer : AOrderComparer<AInternalOrderItem> {
		public override bool AreEqual(AInternalOrderItem a, AInternalOrderItem b) {
			return a.NativeOrderId == b.NativeOrderId;
		} // AreEqual

		public override int HashCode(AInternalOrderItem a) {
			return a.NativeOrderId.GetHashCode();
		} // HashCode
	} // class InternalOrderComparer

	#endregion class InternalOrderComparer

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

	#region class YodleeOrderComparer

	class YodleeOrderComparer : AOrderComparer<BankTransactionData>
	{
		public override bool AreEqual(BankTransactionData a, BankTransactionData b)
		{
			return (a.srcElementId == b.srcElementId);
		} // AreEqual

		public override int HashCode(BankTransactionData a)
		{
			return a.srcElementId.GetHashCode();
		} // HashCode
	} // class YodleeOrderComparer

	class YodleeTransactionComparer: AOrderComparer<MP_YodleeOrderItemBankTransaction>
	{
		public override bool AreEqual(MP_YodleeOrderItemBankTransaction a, MP_YodleeOrderItemBankTransaction b)
		{
			return a.srcElementId == b.srcElementId;
		}

		public override int HashCode(MP_YodleeOrderItemBankTransaction a)
		{
			return a.srcElementId.GetHashCode();
		}
	}
	#endregion class YodleeOrderComparer
} // namespace EZBob.DatabaseLib
