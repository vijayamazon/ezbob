namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class AddInvestorBankAccountBalance : AStrategy{
		private readonly int bankAccountID;
		private readonly DateTime date;
		private readonly decimal transacionAmount;
		private readonly int? userID;
		private readonly string comment;
		private readonly DateTime? transactionDate;
		private readonly string bankTransactionRef;

		public AddInvestorBankAccountBalance(int bankAccountID, DateTime date, decimal transacionAmount, int? userID, string comment, DateTime? transactionDate, string bankTransactionRef) {
			this.bankAccountID = bankAccountID;
			this.date = date;
			this.transacionAmount = transacionAmount;
			this.userID = userID;
			this.comment = comment;
			this.transactionDate = transactionDate;
			this.bankTransactionRef = bankTransactionRef;
		}

		public override string Name { get { return "Add bank account balance"; } }

		public override void Execute() {
			BankAccountTransactionID = DB.ExecuteScalar<int>("I_InvestorBankAccountTransactionAdd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("BankAccountID", this.bankAccountID),
					new QueryParameter("Date", this.date),
					new QueryParameter("TransactionAmount", this.transacionAmount),
					new QueryParameter("UserID", this.userID),
					new QueryParameter("Comment", this.comment),
					new QueryParameter("TransactionDate", this.transactionDate),
					new QueryParameter("BankTransactionRef", this.bankTransactionRef));
		}

		public int BankAccountTransactionID { get; private set; }
	}
}
