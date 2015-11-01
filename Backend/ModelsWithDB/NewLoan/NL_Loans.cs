namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Loans : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanID { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long OfferID { get; set; }

		[FK("LoanType", "Id")]
		[DataMember]
		[EnumName(typeof(NLLoanTypes))]
		public int LoanTypeID { get; set; }

		[FK("NL_LoanStatuses", "LoanStatusID")]
		[DataMember]
		[EnumName(typeof(NLLoanStatuses))]
		public int LoanStatusID { get; set; }

		[FK("NL_LoanFormulas", "FormulaID")]
		[DataMember]
		[EnumName(typeof(NLLoanFormulas))]
		public int LoanFormulaID { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		[EnumName(typeof(NLLoanSources))]
		public int LoanSourceID { get; set; }

		[FK("NL_EzbobBankAccounts", "EzbobBankAccountID")]
		[DataMember]
		public int? EzbobBankAccountID { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[Length(50)]
		[DataMember]
		public string Refnum { get; set; }

		[DataMember]
		public DateTime RepaymentDate { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public DateTime? DateClosed { get; set; }

		[DataMember]
		public long PrimaryLoanID { get; set; }

		[DataMember]
		public decimal? PaymentPerInterval { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public int? OldLoanID { get; set; }

		// additions
		private List<NL_LoanHistory> _histories = new List<NL_LoanHistory>();
		private List<NL_LoanFees> _fees = new List<NL_LoanFees>();
		private List<NL_LoanInterestFreeze> _freezeInterestIntervals = new List<NL_LoanInterestFreeze>();
		private NL_LoanOptions _loanOptions = new NL_LoanOptions();
		private List<NL_Payments> _payments = new List<NL_Payments>();
	
		[DataMember]
		[NonTraversable]
		public List<NL_LoanHistory> Histories {
			get { return this._histories; }
			set { this._histories = value; }
		}

		[DataMember]
		[NonTraversable]
		public List<NL_LoanFees> Fees {
			get { return this._fees; }
			set { this._fees = value; }
		}

		[DataMember]
		[NonTraversable]
		public List<NL_LoanInterestFreeze> FreezeInterestIntervals {
			get { return this._freezeInterestIntervals; }
			set { this._freezeInterestIntervals = value; }
		}

		[DataMember]
		[NonTraversable]
		public NL_LoanOptions LoanOptions {
			get { return this._loanOptions; }
			set { this._loanOptions = value; }
		}

		[DataMember]
		[NonTraversable]
		public List<NL_Payments> Payments {
			get { return this._payments; }
			set { this._payments = value; }
		}


		// helpers
		public NL_LoanHistory LastHistory() {
			return this._histories.OrderBy(h => h.EventTime).LastOrDefault();
		}

		public NL_LoanHistory FirstHistory() {
			return this._histories.OrderBy(h => h.EventTime).FirstOrDefault();
		}

		// +month|+7 days
		public void SetDefaultRepaymentDate() {
			if (RepaymentDate == DateTime.MinValue) {
				NL_LoanHistory lastHistory = LastHistory() ?? new NL_LoanHistory();
				RepaymentDate = (lastHistory.RepaymentIntervalTypeID == (int)RepaymentIntervalTypes.Month) ? lastHistory.EventTime.Date.AddMonths(1) : lastHistory.EventTime.Date.AddDays(7);
			}
		}

		public void SetDefaultFormula() {
			if (LoanFormulaID == 0) 
				LoanFormulaID = (int)NLLoanFormulas.EqualPrincipal;
		}

		public void SetDefaultLoanType() {
			if (LoanTypeID == 0)
				LoanTypeID = (int)NLLoanTypes.StandardLoanType;
		}

		public void SetDefaults() {
			SetDefaultRepaymentDate();
			SetDefaultFormula();
			SetDefaultLoanType();
		}

		
		
		public override string ToString() {
			// loan
			StringBuilder sb = new StringBuilder().Append(base.ToString()); //.Append(Environment.NewLine);

			sb.Append("Fees:");

			// fees
			if (Fees.Count > 0) {
				sb.Append(Environment.NewLine).Append(GetHeadersLine(typeof(NL_LoanFees)));
				Fees.ForEach(s => sb.Append(s.ToString()));
			}  

			sb.Append("Histories:");

			// histories
			if (Histories != null) {
				sb.Append(Environment.NewLine);
				Histories.ForEach(h => sb.Append(h.ToString()));
			}

			sb.Append(Environment.NewLine).Append("Payments:");

			if (Payments.Count > 0) {
				//sb.Append(Environment.NewLine).Append(GetHeadersLine(typeof(NL_Payments)));
				Payments.ForEach(s => sb.Append(Environment.NewLine).Append(GetHeadersLine(typeof(NL_Payments))).Append(s.ToString()));
			}

			sb.Append(Environment.NewLine).Append("LoanInterestFreeze:");

			// freeze interest intervals
			if (FreezeInterestIntervals.Count > 0) {
				sb.Append(Environment.NewLine).Append(GetHeadersLine(typeof(NL_LoanInterestFreeze)));
				FreezeInterestIntervals.ForEach(s => sb.Append(s.ToString()));
			}
			
			sb.Append(Environment.NewLine).Append("Loan options:");

			if (LoanOptions.LoanOptionsID > 0) {
				sb.Append(Environment.NewLine).Append(GetHeadersLine(typeof(NL_LoanOptions)));
				sb.Append(LoanOptions);
			} 
		
			return sb.ToString();
		}

	} // class NL_Loans
} // ns
