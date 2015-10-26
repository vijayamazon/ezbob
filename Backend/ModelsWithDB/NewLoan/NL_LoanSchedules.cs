namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
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
		[EnumName(typeof(NLScheduleStatuses))]
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
		[DecimalFormat("F6")]
		public decimal InterestRate { get; set; }


		// additions
		private decimal _balance;
		private decimal _interest;
		private decimal _feesAmount;
		private decimal _amountDue;
		private decimal _interestPaid;
		private decimal _feesPaid;

		[DataMember]
		[NonTraversable]
		public decimal Balance {
			get { return this._balance; }
			set { this._balance = value; }
		}

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

		public new const string propertyTab = "\t";
	
		/// <summary>
		/// prints data only
		/// to print headers line call base static GetHeadersLine 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}

		public string ToBaseString() {
			return base.ToString();
		}

	} // class NL_LoanSchedules
} // ns
