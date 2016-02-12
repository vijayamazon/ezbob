namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.MainStrategy.Exceptions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using SalesForceLib.Models;

	internal class CreateFindCashRequest : AOneExitStep {
		public CreateFindCashRequest(
			string outerContextDescription,
			long cashRequestID,
			CashRequestOriginator? cashRequestOriginator,
			int customerID,
			string customerFullName,
			string customerEmail,
			string customerOrigin,
			int customerNumOfLoans,
			int underwriterID
		) : base(outerContextDescription) {
			CashRequestID = cashRequestID;
			NLCashRequestID = 0;

			this.customerID = customerID;
			this.customerFullName = customerFullName;
			this.customerEmail = customerEmail;
			this.customerOrigin = customerOrigin;
			this.customerNumOfLoans = customerNumOfLoans;
			this.underwriterID = underwriterID;
			this.cashRequestOriginator = cashRequestOriginator ?? CashRequestOriginator.Other;

			this.transaction = null;
		} // constructor

		[StepOutput]
		public long CashRequestID { get; private set; }

		[StepOutput]
		public long NLCashRequestID { get; private set; }

		[StepOutput]
		public bool HasCashRequest { get { return true; } }

		protected override void ExecuteStep() {
			if (CashRequestID.HasValue())
				Find();
			else
				Create();

			if (CashRequestID.LacksValue() || NLCashRequestID.LacksValue()) {
				throw new CreateFindCashRequestException(
					"No cash request to update (neither specified nor created) for {0}.",
					OuterContextDescription
				);
			} // if
		} // ExecuteStep

		private void Create() {
			ReleaseOpenPlatfromOffer();

			try {
				this.transaction = DB.GetPersistentTransaction();

				SafeReader sr = DB.GetFirst(
					this.transaction,
					"MainStrategyCreateCashRequest",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerID),
					new QueryParameter("@Now", DateTime.UtcNow),
					new QueryParameter("@Originator", this.cashRequestOriginator.ToString())
				);

				if (!sr.IsEmpty) {
					CashRequestID = sr["CashRequestID"];

					NLCashRequestAdd();

					int cashRequestCount = sr["CashRequestCount"];
					decimal? lastLoanAmount = sr["LastLoanAmount"];

					this.transaction.Commit();

					AddOpportunity(cashRequestCount, lastLoanAmount);
				} else
					this.transaction.Rollback();
			} catch (Exception e) {
				if (this.transaction != null)
					this.transaction.Rollback();

				Log.Alert(e, "Failed to create cash request.");
				CashRequestID = 0;
				NLCashRequestID = 0;
			} // try
		}// Create

		private void ReleaseOpenPlatfromOffer() {
			try {
				switch (this.cashRequestOriginator) {
					case CashRequestOriginator.NewCreditLineBtn:
					case CashRequestOriginator.NewCreditLineSkipAll:
					case CashRequestOriginator.NewCreditLineSkipAndGoAuto:
					case CashRequestOriginator.NewCreditLineUpdateAndGoAuto:
					case CashRequestOriginator.NewCreditLineUpdateAndGoManual:
						DB.ForEachRowSafe(HandleOneExpired, "I_GetLastCashRequestExpired", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.customerID));
						
						break;
				default:
						return;
				}//switch
			} catch (Exception ex) {
				Log.Error(ex, "Failed to release the open platform previous offer for customer {0}", this.customerID);
			}//try
		}//ReleaseOpenPlatfromOffer

		private ActionResult HandleOneExpired(SafeReader sr, bool arg2) {
			bool isOP = sr["IsOpenPlatform"];
			bool isApproved = sr["IsApproved"];
			if (isApproved && isOP) {
				OfferExpiredChecker checker = new OfferExpiredChecker();
				long cashRequestID = sr["CashRequestID"];
				checker.MarkOfferAsExpired(cashRequestID);
				checker.UpdateSystemBalance(sr["InvestorID"], 
					sr["FundingBankAccountID"], 
					sr["CreditSum"], 
					sr["InvestmentPercent"], 
					cashRequestID);
			}
			return ActionResult.Continue;
		}//HandleOneExpired


		
		private void AddOpportunity(int cashRequestCount, decimal? lastLoanAmount) {
			bool addOpportunity = 
				(this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) &&
				(this.cashRequestOriginator != CashRequestOriginator.ForcedWizardCompletion) &&
				(cashRequestCount > 1);

			if (!addOpportunity)
				return;

			new AddOpportunity(this.customerID,
				new OpportunityModel {
					Email = this.customerEmail,
					Origin = this.customerOrigin,
					CreateDate = DateTime.UtcNow,
					ExpectedEndDate = DateTime.UtcNow.AddDays(7),
					RequestedAmount = lastLoanAmount.HasValue ? (int)lastLoanAmount.Value : (int?)null,
					Type =
						(this.customerNumOfLoans == 0 ? OpportunityType.New : OpportunityType.Resell).DescriptionAttr(),
					Stage = OpportunityStage.s5.DescriptionAttr(),
					Name = this.customerFullName + cashRequestCount
				}
			).Execute();
		} // AddOpportunity

		private void Find() {
			try {
				this.transaction = DB.GetPersistentTransaction();

				DB.ExecuteNonQuery(
					this.transaction,
					"MainStrategySetCustomerIsBeingProcessed",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerID)
				);

				NLCashRequestID = DB.ExecuteScalar<long>(
					this.transaction,
					"NL_CashRequestGetByOldID",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@OldCashRequestID", CashRequestID)
				);

				if (NLCashRequestID.LacksValue())
					NLCashRequestAdd();

				this.transaction.Commit();
			} catch (Exception e) {
				if (this.transaction != null)
					this.transaction.Rollback();

				Log.Alert(e, "Failed to find cash request.");
				CashRequestID = 0;
				NLCashRequestID = 0;
			} // try
		} // Find

		private void NLCashRequestAdd() {
			AddCashRequest nlAddCashRequest = new AddCashRequest(new NL_CashRequests {
				CashRequestOriginID = (int)this.cashRequestOriginator,
				CustomerID = this.customerID,
				OldCashRequestID = CashRequestID,
				RequestTime = DateTime.UtcNow,
				UserID = this.underwriterID,
			}) {
				Transaction = this.transaction,
			};
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

		private ConnectionWrapper transaction;
	} // class CreateFindCashRequest
} // namespace
