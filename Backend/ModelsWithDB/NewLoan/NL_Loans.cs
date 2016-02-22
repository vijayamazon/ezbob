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
		public int LoanFormulaID {
			get { return this.loanFormulaID; }
			set { this.loanFormulaID = value; }
		}

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
		public int Position { get; set; }

		[DataMember]
		public DateTime? DateClosed { get; set; }

		[DataMember]
		public long PrimaryLoanID { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public int? OldLoanID { get; set; }

		private int loanFormulaID = (int)NLLoanFormulas.EqualPrincipal;

		// additions
		private List<NL_LoanHistory> _histories = new List<NL_LoanHistory>();
		private List<NL_LoanFees> _fees = new List<NL_LoanFees>();
		private List<NL_LoanInterestFreeze> _freezeInterestIntervals = new List<NL_LoanInterestFreeze>();
		private NL_LoanOptions _loanOptions = new NL_LoanOptions();
		private List<NL_Payments> _payments = new List<NL_Payments>();
		private List<NL_LoanRollovers> _acceptedRollovers = new List<NL_LoanRollovers>();
		private List<NL_LoanRollovers> _rollovers = new List<NL_LoanRollovers>();
	
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
		
		[DataMember]
		[NonTraversable]
		public List<NL_LoanRollovers> AcceptedRollovers {
			get { return this._acceptedRollovers; }
			set { this._acceptedRollovers = value; }
		}

		[DataMember]
		[NonTraversable]
		public List<NL_LoanRollovers> Rollovers {
			get { return this._rollovers; }
			set { this._rollovers = value; }
		}

		// helpers
		public NL_LoanHistory LastHistory() {
			return this._histories.OrderBy(h => h.EventTime).LastOrDefault();
		}

		public NL_LoanHistory FirstHistory() {
			return this._histories.OrderBy(h => h.EventTime).FirstOrDefault();
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
			SetDefaultFormula();
		//	SetDefaultLoanType();
		}

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public override string ToString() {
			// loan
			StringBuilder sb = new StringBuilder().Append(PrintHeadersLine(typeof(NL_Loans))).Append(ToStringAsTable()).Append(Environment.NewLine);

			// freeze interest intervals
			if (FreezeInterestIntervals.Count > 0) {
				sb.Append("LoanInterestFreeze:")
					.Append(Environment.NewLine)
					.Append(PrintHeadersLine(typeof(NL_LoanInterestFreeze)));
				FreezeInterestIntervals.ForEach(s => sb.Append(s.ToString()));
			} //else sb.Append("No LoanInterestFreeze").Append(Environment.NewLine);

			if (LoanOptions.LoanOptionsID > 0) {
				sb.Append("LoanOptions:")
					.Append(Environment.NewLine)
					.Append(PrintHeadersLine(typeof(NL_LoanOptions)))
					.Append(LoanOptions.ToStringAsTable());
			} //else sb.Append("No Loan options").Append(Environment.NewLine);

			// rollovers
			if (AcceptedRollovers.Count > 0) {
				sb.Append(Environment.NewLine)
					.Append("AcceptedRollovers:").Append(Environment.NewLine)
					.Append(PrintHeadersLine(typeof(NL_LoanRollovers)));
				AcceptedRollovers.ForEach(r => sb.Append(r.ToStringAsTable()));
			} //else sb.Append("No AcceptedRollovers").Append(Environment.NewLine);

			// fees
			if (Fees.Count > 0) {
				sb.Append(Environment.NewLine)
					.Append("Fees:").Append(Environment.NewLine)
					.Append(PrintHeadersLine(typeof(NL_LoanFees)));
				Fees.ForEach(s => sb.Append(s.ToStringAsTable()));
			} //else sb.Append("No Fees").Append(Environment.NewLine);

			// histories
			if (Histories != null) {
				sb.Append(Environment.NewLine)
					.Append("Histories:").Append(Environment.NewLine);
				Histories.ForEach(h => sb.Append(h.ToString()));
			}  // else sb.Append("No Histories").Append(Environment.NewLine);

			// payments
			if (Payments.Count > 0) {
				sb.Append("Payments:");
				Payments.ForEach(p => sb.Append(p.ToString()));
			} // else sb.Append("No Payments").Append(Environment.NewLine);
		
			return sb.ToString();
		}

	} // class NL_Loans
} // ns
