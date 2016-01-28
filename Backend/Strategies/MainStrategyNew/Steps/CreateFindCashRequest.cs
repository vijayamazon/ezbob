namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MainStrategyNew.Exceptions;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using SalesForceLib.Models;

	internal class CreateFindCashRequest : AOneExitStep {
		public CreateFindCashRequest(
			string outerContextDescription,
			AMainStrategyStep nextStep,
			InternalCashRequestID cashRequestID,
			CashRequestOriginator? cashRequestOriginator,
			int customerID,
			string customerFullName,
			string customerEmail,
			string customerOrigin,
			int customerNumOfLoans,
			int underwriterID
		) : base(outerContextDescription, nextStep) {
			CashRequestID = cashRequestID;
			NLCashRequestID = 0;

			this.customerID = customerID;
			this.customerFullName = customerFullName;
			this.customerEmail = customerEmail;
			this.customerOrigin = customerOrigin;
			this.customerNumOfLoans = customerNumOfLoans;
			this.underwriterID = underwriterID;
			this.cashRequestOriginator = cashRequestOriginator ?? CashRequestOriginator.Other;
		} // constructor

		[StepOutput]
		public InternalCashRequestID CashRequestID { get; private set; }

		[StepOutput]
		public long NLCashRequestID { get; private set; }

		protected override void ExecuteStep() {
			if (CashRequestID.HasValue) {
				DB.ExecuteNonQuery(
					"MainStrategySetCustomerIsBeingProcessed",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerID)
				);

				NLCashRequestID = DB.ExecuteScalar<long>(
					"NL_CashRequestGetByOldID",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@OldCashRequestID", CashRequestID.Value)
				);

				if (NLCashRequestID == 0L)
					NLCashRequestAdd();

				return;
			} // if

			SafeReader sr = DB.GetFirst(
				"MainStrategyCreateCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID),
				new QueryParameter("@Now", DateTime.UtcNow),
				new QueryParameter("@Originator", this.cashRequestOriginator.ToString())
			);

			if (sr.IsEmpty)
				throw new CreateFindCashRequestException("Cash request was not created for customer {0}.", this.customerID);

			CashRequestID.Value = sr["CashRequestID"];

			NLCashRequestAdd();

			int cashRequestCount = sr["CashRequestCount"];

			bool addOpportunity = 
				(this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) &&
				(this.cashRequestOriginator != CashRequestOriginator.ForcedWizardCompletion) &&
				(cashRequestCount > 1);

			if (addOpportunity) {
				decimal? lastLoanAmount = sr["LastLoanAmount"];

				new AddOpportunity(this.customerID,
					new OpportunityModel {
						Email = this.customerEmail,
						Origin = this.customerOrigin,
						CreateDate = DateTime.UtcNow,
						ExpectedEndDate = DateTime.UtcNow.AddDays(7),
						RequestedAmount = lastLoanAmount.HasValue ? (int)lastLoanAmount.Value : (int?)null,
						Type = this.customerNumOfLoans == 0
							? OpportunityType.New.DescriptionAttr()
							: OpportunityType.Resell.DescriptionAttr(),
						Stage = OpportunityStage.s5.DescriptionAttr(),
						Name = this.customerFullName + cashRequestCount
					}
				).Execute();
			} // if

			if (CashRequestID.LacksValue) {
				throw new CreateFindCashRequestException(
					"No cash request to update (neither specified nor created) for {0}.",
					OuterContextDescription
				);
			} // if
		} // ExecuteStep

		private void NLCashRequestAdd() {
			AddCashRequest nlAddCashRequest = new AddCashRequest(new NL_CashRequests {
				CashRequestOriginID = (int)this.cashRequestOriginator,
				CustomerID = this.customerID,
				OldCashRequestID = CashRequestID,
				RequestTime = DateTime.UtcNow,
				UserID = this.underwriterID,
			});
			nlAddCashRequest.Context.CustomerID = this.customerID;
			nlAddCashRequest.Context.UserID = this.underwriterID;
			nlAddCashRequest.Execute();

			NLCashRequestID = nlAddCashRequest.CashRequestID;
		} // NLCashRequestAdd

		private readonly int customerID;
		private readonly int underwriterID;
		private readonly string customerFullName;
		private readonly string customerEmail;
		private readonly string customerOrigin;
		private readonly int customerNumOfLoans;
		private readonly CashRequestOriginator cashRequestOriginator;
	} // class CreateFindCashRequest
} // namespace
