namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class LoanMetaData : AResultRow {
		public LoanMetaData() {
			AssumedLoanAmount = null;
			Cap = null;
			MaxOffer = new MaxOfferValues(this);
			MinOffer = new MinOfferValues(this);
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

		[NonTraversable]
		public decimal? Cap {
			get { return this.cap; }
			set { this.cap = value <= 0 ? 0 : value; }
		} // Cap

		[NonTraversable]
		public decimal? AssumedLoanAmount { get; set; }

		public class MaxOfferValues {
			public MaxOfferValues(LoanMetaData lmd) {
				this.lmd = lmd;
			} // constructor

			public decimal LoanAmount {
				get {
					decimal loanAmount = this.lmd.AssumedLoanAmount ?? this.lmd.LoanAmount;

					return this.lmd.Cap.HasValue ? Math.Min(loanAmount, this.lmd.Cap.Value) : loanAmount;
				} // get
			} // LoanAmount

			public decimal RepaidPrincipal {
				get {
					decimal repaidPrincipal;

					if (this.lmd.AssumedLoanAmount.HasValue) {
						repaidPrincipal = this.lmd.LoanAmount == 0
							? this.lmd.RepaidPrincipal
							: this.lmd.RepaidPrincipal * this.lmd.AssumedLoanAmount.Value / this.lmd.LoanAmount;
					} else
						repaidPrincipal = this.lmd.RepaidPrincipal;

					return this.lmd.Cap.HasValue ? Math.Min(repaidPrincipal, this.lmd.Cap.Value) : repaidPrincipal;
				} // get
			} // RepaidPrincipal

			private readonly LoanMetaData lmd;
		} // class MaxOfferValues

		public class MinOfferValues {
			public MinOfferValues(LoanMetaData lmd) {
				this.lmd = lmd;
			} // constructor

			public decimal LoanAmount {
				get {
					return this.lmd.Cap.HasValue
						? Math.Min(this.lmd.LoanAmount, this.lmd.Cap.Value)
						: this.lmd.LoanAmount;
				} // get
			} // LoanAmount

			public decimal RepaidPrincipal {
				get {
					return this.lmd.Cap.HasValue
						? Math.Min(this.lmd.RepaidPrincipal, this.lmd.Cap.Value)
						: this.lmd.RepaidPrincipal;
				} // get
			} // RepaidPrincipal

			private readonly LoanMetaData lmd;
		} // class MinOfferValues

		public MinOfferValues MinOffer { get; private set; }

		public MaxOfferValues MaxOffer { get; private set; }

		public LoanMetaData Clone() {
			return new LoanMetaData {
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

				Cap = Cap,
				AssumedLoanAmount = AssumedLoanAmount,
			};
		} // Clone

		private decimal? cap;
	} // class LoanMetaData
} // namespace
