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

		[FK("NL_EzbobBankAccounts", "EzbobBankAccountID")]
		[DataMember]
		public int? EzbobBankAccountID { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		[EnumName(typeof(NLLoanSources))]
		public int LoanSourceID { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[Length(50)]
		[DataMember]
		public string Refnum { get; set; }

		[DataMember]
		public DateTime? DateClosed { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public int? OldLoanID { get; set; }



		// additions
		private List<NL_LoanHistory> _histories = new List<NL_LoanHistory>();
		private List<NL_LoanFees> _fees = new List<NL_LoanFees>();

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

		// helpers
		public NL_LoanHistory LastHistory() {
			return this._histories.OrderBy(h => h.EventTime).LastOrDefault();
		}

		public NL_LoanHistory FirstHistory() {
			return this._histories.OrderBy(h => h.EventTime).FirstOrDefault();
		}

		public override string ToString() {
			// loan
			StringBuilder sb = new StringBuilder().Append(base.ToString()).Append(Environment.NewLine);

			// fees
			sb.Append(HeadersLine(typeof(NL_LoanFees), NL_LoanFees.ColumnTotalWidth));
			if (Fees.Count>0)
				Fees.ForEach(s => sb.Append(s.ToString()));

			sb.Append(Environment.NewLine);
			// histories
			if(Histories!=null)
				Histories.ForEach(h => sb.Append(h.ToString()));

			return sb.ToString();
		}

	} // class NL_Loans
} // ns
