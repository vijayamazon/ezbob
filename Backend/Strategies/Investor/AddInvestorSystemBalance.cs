﻿namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class AddInvestorSystemBalance : AStrategy {
		private readonly int bankAccountID;
		private readonly DateTime date;
		private readonly decimal transacionAmount;
		private readonly decimal? servicingFeeAmount;
		private readonly long? cashRequestID;
		private readonly int? loanID;
		private readonly int? loanTransactionID;
		private readonly string comment;

		public AddInvestorSystemBalance(int bankAccountID, 
			DateTime date, 
			decimal transacionAmount, 
			decimal? servicingFeeAmount, 
			long? cashRequestID, 
			int? loanID, 
			int? loanTransactionID, 
			string comment) {
			this.bankAccountID = bankAccountID;
			this.date = date;
			this.transacionAmount = transacionAmount;
			this.servicingFeeAmount = servicingFeeAmount;
			this.cashRequestID = cashRequestID;
			this.loanID = loanID;
			this.loanTransactionID = loanTransactionID;
			this.comment = comment;
		}

		public override string Name { get { return "Add investor system balance"; } }

		public override void Execute() {
			SystemBalanceID = DB.ExecuteScalar<int>("I_SystemBalanceAdd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("BankAccountID", this.bankAccountID),
					new QueryParameter("Date", this.date),
					new QueryParameter("TransactionAmount", this.transacionAmount),
					new QueryParameter("ServicingFeeAmount", this.servicingFeeAmount),
					new QueryParameter("LoanTransactionID", this.loanTransactionID),
					new QueryParameter("LoanID", this.loanID),
					new QueryParameter("CashRequestID", this.cashRequestID),
					new QueryParameter("Comment", this.comment));
		}

		public int SystemBalanceID { get; private set; }
	}
}


