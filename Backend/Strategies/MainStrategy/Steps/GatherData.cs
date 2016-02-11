namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;

	internal class GatherData : AOneExitStep {
		public GatherData(string outerContextDescription, int customerID) : base(outerContextDescription) {
			this.customerID = customerID;
		} // constructor

		[StepOutput]
		public TypeOfBusiness TypeOfBusiness { get; private set; }

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

			TypeOfBusiness tob;

			TypeOfBusiness = Enum.TryParse(sp.TypeOfBusiness, true, out tob) ? tob : TypeOfBusiness.Entrepreneur;

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
