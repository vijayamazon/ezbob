namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;

	public class AddManualTransaction : AStrategy {
		public AddManualTransaction(int underwriterID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, int bankAccountTypeID, string transactionComment, string bankTransactionRef) {
		this.underwriterID = underwriterID;
			this.investorAccountID = investorAccountID;
			this.transactionAmount = transactionAmount;
			this.transactionDate = transactionDate;
			this.bankAccountTypeID = bankAccountTypeID;
			this.transactionComment = transactionComment;
			this.now = DateTime.UtcNow;
			this.bankTransactionRef = bankTransactionRef;
		}//ctor

		public override string Name { get { return "AddManualTransaction"; } }

		public override void Execute() {

			try {

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Funding) {
					this.systemBalanceTransactionSign = 1;
					this.bankAccountType = I_InvestorAccountTypeEnum.Funding;
				}


				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Repayments) {
					this.systemBalanceTransactionSign = -1;
					this.bankAccountType = I_InvestorAccountTypeEnum.Repayments;
				}

				AddInvestorSystemBalance changeSystemBalance = new AddInvestorSystemBalance(this.investorAccountID,
					this.now,
					this.transactionAmount * this.systemBalanceTransactionSign,
					null,
					null,
					null,
					null,
					this.transactionComment,
					this.underwriterID,
					this.transactionDate);
				changeSystemBalance.Execute();

				AddInvestorBankAccountBalance addBankAccountBalance = new AddInvestorBankAccountBalance(this.investorAccountID,
					this.now,
					this.transactionAmount,
					this.underwriterID,
					this.transactionComment,
					this.transactionDate,
					this.bankTransactionRef);
				addBankAccountBalance.Execute();

			} catch (Exception ex) {
				Log.Warn(ex, "Failed to execute {0} transfer of £{1} for investor bank account {2} into DB", this.bankAccountType, this.transactionAmount, this.investorAccountID);
				Result = false;
				throw;
			}

			Result = true;
			Log.Info("Executing {0} transfer of £{1} for investor bank account {2} into DB complete.", this.bankAccountType, this.transactionAmount, this.investorAccountID);
		}//Execute

		public bool Result { get; set; }
		private readonly int underwriterID;
		private readonly int investorAccountID;
		private readonly int bankAccountTypeID;
		private I_InvestorAccountTypeEnum bankAccountType;
		private readonly decimal transactionAmount;
		private readonly DateTime transactionDate;
		private readonly string transactionComment;
		private int systemBalanceTransactionSign;
		private readonly DateTime now;
		private readonly string bankTransactionRef;
	}//AddManualTransaction
}//ns


