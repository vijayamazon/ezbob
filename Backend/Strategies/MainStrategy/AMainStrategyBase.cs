namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Lingvo;

	public abstract class AMainStrategyBase : AStrategy {
		public abstract int CustomerID { get; }

		protected virtual void LoadCompanyAndMonthlyPayment(DateTime now) {
			var sp = new GetCustomerCompanyID(DB, Log) {
				CustomerID = CustomerID,
				Now = now,
			};

			sp.ExecuteNonQuery();

			CompanyID = sp.CompanyID;

			MonthlyRepayment = InjectorStub.GetEngine().GetMonthlyRepaymentData(CustomerID, now);

			Log.Debug(
				"Customer {0} at {1}: company ID is {2}, monthly repayment is {3} (requested {4} for {5}).",
				CustomerID,
				now.MomentStr(),
				CompanyID,
				MonthlyRepayment.MonthlyPayment.ToString("C0"),
				MonthlyRepayment.RequestedAmount.ToString("C0"),
				Grammar.Number(MonthlyRepayment.RequestedTerm, "month")
			);
		} // LoadCompanyAndMonthlyPayment

		protected virtual int CompanyID { get; private set; }
		protected virtual MonthlyRepaymentData MonthlyRepayment { get; private set; }
	} // class AMainStrategyBase
} // namespace
