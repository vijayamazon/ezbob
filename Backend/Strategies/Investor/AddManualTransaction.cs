namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class AddManualTransaction : AStrategy {
		public AddManualTransaction(int underwriterID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, int bankAccountTypeID, string transactionComment) {
		this.underwriterID = underwriterID;
			this.investorAccountID = investorAccountID;
			this.transactionAmount = transactionAmount;
			this.transactionDate = transactionDate;
			this.bankAccountTypeID = bankAccountTypeID;
			this.transactionComment = transactionComment;
		}//ctor

		public override string Name { get { return "AddManualTransaction"; } }

		public override void Execute() {
			 
			var con = DB.GetPersistent();
			con.BeginTransaction();

			try {

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Funding) {
					this.spName = "I_InvestorBankAccountTransactionSave";

					var systemBalanceID = DB.ExecuteScalar<int>("I_SystemBalanceAdd",
						CommandSpecies.StoredProcedure,
						new QueryParameter("BankAccountID", this.investorAccountID),
						new QueryParameter("Date", this.transactionDate),
						new QueryParameter("TransactionAmount", this.transactionAmount),
						new QueryParameter("ServicingFeeAmount", null),
						new QueryParameter("LoanTransactionID", null));

					var bankAccountTransactionID = DB.ExecuteScalar<int>("I_SystemBalanceAdd",
						CommandSpecies.StoredProcedure,
						new QueryParameter("BankAccountID", this.investorAccountID),
						new QueryParameter("Date", this.transactionDate),
						new QueryParameter("TransactionAmount", this.transactionAmount),
						new QueryParameter("ServicingFeeAmount", null),
						new QueryParameter("LoanTransactionID", null));

				}

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Repayments) {
					this.spName = "I_InvestorSystemBalanceSave";
					var dbBank = new I_InvestorSystemBalance {
						InvestorBankAccountID = this.investorAccountID,
						Timestamp = this.transactionDate,
						TransactionAmount = this.transactionAmount,
					};

					DB.ExecuteNonQuery(con, this.spName, CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<I_InvestorSystemBalance>("Tbl", new List<I_InvestorSystemBalance> { dbBank })
					);

				}


			} catch (Exception ex) {
				Log.Warn(ex, "Failed to add {0} transaction for investor bank account with ID {1} to DB", this.bankAccountTypeID, this.investorAccountID);
				con.Rollback();
				Result = false;
				throw;
			}

			con.Commit();
			Result = true;
			Log.Info("Adding {0} transaction for investor bank account with ID {1} into DB complete.", this.bankAccountTypeID, this.investorAccountID);
		}//Execute

		public bool Result { get; set; }
		private readonly int underwriterID;
		private readonly int investorAccountID;
		private readonly int bankAccountTypeID;
		private readonly decimal transactionAmount;
		private readonly DateTime transactionDate;
		private readonly string transactionComment;
		private string spName;
	}//ManageInvestorBankAccount   transactionComment
}//ns


