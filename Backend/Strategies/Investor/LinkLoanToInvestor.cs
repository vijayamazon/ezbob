namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	public class LinkLoanToInvestor : AStrategy {

		public LinkLoanToInvestor(int customerID, int loanID) {
			this.customerID = customerID;
			this.loanID = loanID;
			this.now = DateTime.UtcNow;
		} //ctor

		public override string Name { get { return "LinkLoanToInvestor"; } }

		public override void Execute() {
			IsForOpenPlatform = DB.ExecuteScalar<bool>("I_LoanForOpenPlatform",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanID", this.loanID));
			
			if (IsForOpenPlatform) {
				GetLatestKnownInference inference = new GetLatestKnownInference(this.customerID, this.now, false);
				inference.Execute();

				if (inference.Inference != null)
					this.bucket = inference.Inference.Bucket;

				DB.ForEachRowSafe(HandleOneAssignedToLoanInvestor,
					"I_LoadAssigedToLoanInvestors",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanID", this.loanID));
			}//if
		}//Execute

		private ActionResult HandleOneAssignedToLoanInvestor(SafeReader sr, bool bRowSetStart) {
			try {
				int fundingBankAccountID = sr["FundingBankAccountID"];
				decimal investmentPercent = sr["InvestmentPercent"];
				int investorID = sr["InvestorID"];
				int loanTerm = sr["RepaymentPeriod"];
				decimal loanAmount = sr["LoanAmount"];
				int? productTypeID = sr["ProductTypeID"];

				I_Portfolio portfolio = new I_Portfolio {
					InitialTerm = loanTerm,
					InvestorID = investorID,
					ProductTypeID = productTypeID,
					LoanID = this.loanID,
					LoanPercentage = investmentPercent,
					Timestamp = this.now,
					GradeID = this.bucket,
				};

				DB.ExecuteNonQuery("I_PortfolioSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", portfolio));

				const int negative = -1;
				AddInvestorBankAccountBalance addBankAccountBalance = new AddInvestorBankAccountBalance(
					fundingBankAccountID, 
					this.now,
					loanAmount * investmentPercent * negative,
					this.customerID,
					"Loan was taken",
					this.now,
					""
				);
				addBankAccountBalance.Execute();
			} catch (Exception ex) {
				Log.Error(ex, "failed to link loan {0} to investor {1}",
					this.loanID, sr["InvestorID"]);
			}
			return ActionResult.Continue;
		}//HandleOneAssignedToLoanInvestor

		public bool IsForOpenPlatform { get; private set; }

		private readonly int customerID;
		private readonly int loanID;
		private readonly DateTime now;
		private Bucket bucket;
	}//LinkLoanToInvestor
}//ns
