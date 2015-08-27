namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_FundTransfers : AStringable {
		[PK(true)]
		[DataMember]
		public long FundTransferID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public DateTime TransferTime { get; set; }

		[FK("NL_FundTransferStatuses", "FundTransferStatusID")]
		[DataMember]
		public int FundTransferStatusID { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int LoanTransactionMethodID { get; set; }

		[DataMember]
		public DateTime? DeletionTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }


		// additions

		private List<NL_PacnetTransactions> _pacnetTransactions;

		[DataMember]
		[NonTraversable]
		public List<NL_PacnetTransactions> PacnetTransactions {
			get { return this._pacnetTransactions; }
			set { this._pacnetTransactions = value; }
		}

		public NL_PacnetTransactions LastPacnetTransactions() {
			return PacnetTransactions.OrderBy(t => t.TransactionTime)
				.LastOrDefault();
		}
		
	} // class NL_FundTransfers
} // ns