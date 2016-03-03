namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;

	public class MigrationModels {

		public class LoanTransactionModel {
			public DateTime PostDate { get; set; }
			public decimal Amount { get; set; }
			public string Description { get; set; }
			public string IP { get; set; }
			public string PaypointId { get; set; }
			public int CardID { get; set; }
			public long LoanID { get; set; }
			public int LoanTransactionMethodId { get; set; }
			public int CustomerId { get; set; }
		}

		public class CashReqModel {
			public long CashRequestID { get; set; }
			public long DecisionID { get; set; }
			public long OfferID { get; set; }
			public int LegalID { get; set; }
		}

		public class LoanId {
			public int Id { get; set; }
		}
	}
}
