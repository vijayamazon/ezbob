namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanSchedules : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanScheduleID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("NL_LoanScheduleStatuses", "LoanScheduleStatusID")]
		[DataMember]
		public int LoanScheduleStatusID { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public DateTime PlannedDate { get; set; }

		[DataMember]
		public DateTime? ClosedTime { get; set; }

		[DataMember]
		public decimal Principal { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }



		// additions
		private decimal _interest;
		private decimal _feesAmount ;
		private decimal _amountDue;
		private decimal _balance;
		private decimal _interestPaid ;
		private decimal _feesPaid ;

		[DataMember] // p*r
		[NonTraversable]
		public decimal Interest {
			get { return this._interest; }
			set { this._interest = value; }
		 }

		[DataMember]
		[NonTraversable]
		public decimal FeesAmount {
			get { return this._feesAmount; }
			set { this._feesAmount = value; }
		}

		[DataMember]
		[NonTraversable]
		public decimal AmountDue {
			get { return this._amountDue; }
			set { this._amountDue = value; }
		}

		[DataMember]
		[NonTraversable]
		public decimal InterestPaid {
			get { return this._interestPaid; }
			set { this._interestPaid = value; }
		}

		[DataMember]
		[NonTraversable]
		public decimal FeesPaid {
			get { return this._feesPaid; }
			set { this._feesPaid = value; }
		}

		[DataMember]
		[NonTraversable]
		public decimal Balance {
			get { return this._balance; }
			set { this._balance = value; }
		}

	} // class NL_LoanSchedules
} // ns
