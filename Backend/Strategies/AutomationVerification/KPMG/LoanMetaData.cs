namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class LoanMetaData : AResultRow {
		public LoanMetaData() {
			MaxOffer = new OfferValues(this);
			MinOffer = new OfferValues(this);
		} // LoanMetaData

		public int CashRequestID { get; set; }
		public int CustomerID { get; set; }
		public int LoanID { get; set; }
		public string LoanSourceName { get; set; }
		public DateTime LoanDate { get; set; }
		public DateTime? DateClosed { get; set; }
		public decimal LoanAmount { get; set; }

		public string Status {
			get { return LoanStatus.ToString(); }
			set {
				LoanStatus ls;
				Enum.TryParse(value, true, out ls);
				LoanStatus = ls;
			} // set
		} // Status

		public decimal RepaidPrincipal { get; set; }

		public int MaxLateDays { get; set; }

		public LoanStatus LoanStatus { get; protected set; }

		public class OfferValues {
			public OfferValues(LoanMetaData lmd) {
				Ratio = 1m;
				this.lmd = lmd;
			} // constructor

			public decimal Ratio { get; set; }

			public decimal LoanAmount { get { return this.lmd.LoanAmount * Ratio; } } // LoanAmount

			public decimal RepaidPrincipal { get { return Math.Min(this.lmd.RepaidPrincipal, LoanAmount); } }

			public decimal OutstandingAmount { get { return LoanAmount - RepaidPrincipal; } }

			private readonly LoanMetaData lmd;
		} // class OfferValues

		public OfferValues MinOffer { get; private set; }

		public OfferValues MaxOffer { get; private set; }

		public LoanMetaData Clone() {
			var lmd = new LoanMetaData {
				CashRequestID = CashRequestID,
				CustomerID = CustomerID,
				LoanID = LoanID,
				LoanSourceName = LoanSourceName,
				LoanDate = LoanDate,
				DateClosed = DateClosed,
				LoanAmount = LoanAmount,
				Status = Status,
				RepaidPrincipal = RepaidPrincipal,
				MaxLateDays = MaxLateDays,
			};

			lmd.MinOffer.Ratio = MinOffer.Ratio;
			lmd.MaxOffer.Ratio = MaxOffer.Ratio;

			return lmd;
		} // Clone
	} // class LoanMetaData
} // namespace
