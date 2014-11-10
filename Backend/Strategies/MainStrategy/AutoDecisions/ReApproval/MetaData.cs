namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System;
	using System.Collections.Generic;

	internal class MetaData {
		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		public string RowType { get; set; }
		public int LacrID { get; set; }
		public int RejectAfterLacrID { get; set; }
		public DateTime? LacrTime { get; set; }
		public int LateLoanCount { get; set; }
		public int OpenLoanCount { get; set; }
		public decimal SumOfCharges { get; set; }
		public decimal ManagerApprovedSum { get; set; }
		public decimal TakenLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFees { get; set; }
		public bool? EmailSendingBanned { get; set; }
		public DateTime? OfferValidUntil { get; set; }
		public DateTime? OfferStart { get; set; }

		public bool IsEmailSendingBanned {
			get { return !EmailSendingBanned.HasValue || EmailSendingBanned.Value; } // get
		} // IsEmailSendingBanned

		public double OfferLength {
			get {
				if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
					return 0;

				return (OfferValidUntil.Value - OfferStart.Value).TotalDays;
			} // get
		} // OfferLength

		public List<string> ValidationErrors { get; private set; }

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");

			if (LacrID == 0)
				ValidationErrors.Add("no last manually approved cash request found");

			if (!LacrTime.HasValue)
				ValidationErrors.Add("last approved cash request time not filled");

			if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
				ValidationErrors.Add("last offer start time/length not filled");
		} // Validate

		public int? LacrAge {
			get {
				if (!LacrTime.HasValue)
					return null;

				return (int)(DateTime.UtcNow - LacrTime.Value).TotalDays;
			} // get
		} // LacrAge

		public bool LacrIsTooOld(int nAge) {
			return !LacrAge.HasValue || LacrAge.Value > nAge;
		} // LacrIsTooOld

		public decimal ApprovedAmount {
			get {
				return ManagerApprovedSum - TakenLoanAmount + RepaidPrincipal + SetupFees;
			} // get
		} // ApprovedAmount
	} // class MetaData
} // namespace
