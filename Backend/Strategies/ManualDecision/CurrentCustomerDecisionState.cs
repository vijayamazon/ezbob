namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;

	class CurrentCustomerDecisionState {
		public int UnderwriterID { get; set; }
		public string UnderwriterName { get; set; }
		public bool IsManager { get; set; }

		public int CustomerID { get; set; }
		public string CreditResult { get; set; }
		public int NumOfPrevApprovals { get; set; }
		public int NumOfPrevRejections { get; set; }
		public bool LastWizardStep { get; set; }
		public bool IsAlibaba { get; set; }
		public string Email { get; set; }
		public bool FilledByBroker { get; set; }

		public long CashRequestID { get; set; }
		public int CashRequestCustomerID { get; set; }

		public string DecisionStr {
			get { return Decision.HasValue ? Decision.Value.ToString() : null; }
			set {
				CreditResultStatus crs;
				Decision = Enum.TryParse(value, true, out crs) ? crs : (CreditResultStatus?)null;
			} // set
		} // DecisionStr

		public byte[] CashRequestTimestamp { get; set; }
		public decimal OfferedCreditLine { get; set; }
		public int IsLoanTypeSelectionAllowed { get; set; }
		public bool EmailSendingBanned { get; set; }
		public DateTime OfferValidUntil { get; set; }
		public DateTime OfferStart { get; set; }





		public CreditResultStatus? Decision { get; private set; }

		public bool IsFinalDecision {
			get { return Decision.HasValue && Decision.Value.In(CreditResultStatus.Approved, CreditResultStatus.Rejected); }
		} // IsFinalDecision

		public string CashRequestRowVersion {
			get {
				return (CashRequestTimestamp == null)
					? string.Empty
					: string.Join("", CashRequestTimestamp.Select(b => b.ToString("x2")));
			} // get
		} // CashRequestRowVersion

		public bool CashRequestMatches {
			get { return CustomerID == CashRequestCustomerID; }
		} // CashRequestMatches

		public bool RowVersionChanged(string oldRowVersion) {
			if (string.IsNullOrWhiteSpace(CashRequestRowVersion))
				return true;

			return CashRequestRowVersion != oldRowVersion;
		} // RowVersionChanged
	} // class CurrentCustomerDecisionState
} // namespace
