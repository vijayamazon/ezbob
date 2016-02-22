namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class LinkRepaymentToInvestor : AStrategy {

		public LinkRepaymentToInvestor(
			int loanID,
			int loanTransactionID,
			decimal amount,
			DateTime transactionDate,
			int? userID) {

			this.loanID = loanID;
			this.loanTransactionID = loanTransactionID;
			this.amount = amount;
			this.transactionDate = transactionDate;
			this.userID = userID;
			this.now = DateTime.UtcNow;
		}//ctor

		public override string Name { get { return "LinkRepaymentToInvestor"; } }

		public override void Execute() {
			IsForOpenPlatform = DB.ExecuteScalar<bool>("I_LoanForOpenPlatform",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanID", this.loanID));

			if (IsForOpenPlatform) {
				DB.ForEachRowSafe(HandleOneAssignedToLoanInvestor,
					"I_LoadAssigedToLoanInvestors",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanID", this.loanID));
			}//if
		}//Execute

		private ActionResult HandleOneAssignedToLoanInvestor(SafeReader sr, bool bRowSetStart) {
			try {
				int repaymentBankAccountID = sr["RepaymentBankAccountID"];
				decimal investmentPercent = sr["InvestmentPercent"];
				decimal? discountServicingFeePercent = sr["DiscountServicingFeePercent"];
				decimal servicingFeePercent = ConfigManager.CurrentValues.Instance.InvestorServicingFeePercent;
				if (discountServicingFeePercent.HasValue && discountServicingFeePercent.Value > 0) {
					servicingFeePercent = servicingFeePercent * (1 - discountServicingFeePercent.Value);
				}

				var servicingFeeAmount = this.amount * investmentPercent * servicingFeePercent;
				var investorRepaymentPart = this.amount * investmentPercent - servicingFeeAmount;

				AddInvestorSystemBalance addSystemBalance = new AddInvestorSystemBalance(repaymentBankAccountID,
					this.now,
					investorRepaymentPart,
					servicingFeeAmount,
					null,
					this.loanID,
					this.loanTransactionID,
					"Repayment",
					this.userID, 
					this.transactionDate
				);
				addSystemBalance.Execute();
			} catch (Exception ex) {
				Log.Error(ex, "failed to add system balance for repayment {0} of loan {1} for investor {2}",
					this.loanTransactionID, this.loanID, sr["InvestorID"]);
			}
			return ActionResult.Continue;
		}//HandleOneAssignedToOfferInvestor
		public bool IsForOpenPlatform { get; private set; }

		private readonly int loanID;
		private readonly int loanTransactionID;
		private readonly decimal amount;
		private readonly DateTime transactionDate;
		private readonly int? userID;
		private readonly DateTime now;
	}//LinkRepaymentToInvestor
}//ns
