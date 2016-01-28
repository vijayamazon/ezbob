namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Lingvo;

	internal class GatherData : AOneExitStep {
		public GatherData(
			string outerContextDescription,
			AMainStrategyStep nextStep,
			int customerID
		) : base(outerContextDescription, nextStep) {
			this.customerID = customerID;
		} // constructor

		[StepOutput]
		public int CompanyID { get; private set; }

		[StepOutput]
		public MonthlyRepaymentData MonthlyRepayment { get; private set; }

		protected override void ExecuteStep() {
			DateTime now = DateTime.UtcNow;

			var sp = new GetCustomerCompanyID(DB, Log) {
				CustomerID = this.customerID,
				Now = now,
			};

			sp.ExecuteNonQuery();

			CompanyID = sp.CompanyID;

			MonthlyRepayment = InjectorStub.GetEngine().GetMonthlyRepaymentData(this.customerID, now);

			Log.Debug(
				"Customer {0} at {1}: company ID is {2}, monthly repayment is {3} (requested {4} for {5}).",
				this.customerID,
				now.MomentStr(),
				CompanyID,
				MonthlyRepayment.MonthlyPayment.ToString("C0"),
				MonthlyRepayment.RequestedAmount.ToString("C0"),
				Grammar.Number(MonthlyRepayment.RequestedTerm, "month")
			);
		} // ExecuteStep

		private readonly int customerID;
	} // class GatherData
} // namespace
