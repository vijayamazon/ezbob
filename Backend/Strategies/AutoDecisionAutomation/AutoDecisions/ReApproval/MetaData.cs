namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;

	public class MetaData {
		public string RowType { get; set; }
		public long LacrID { get; set; }
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

		public int FraudStatusValue {
			get { return (int)FraudStatus; }
			set {
				FraudStatus = Enum.IsDefined(typeof(FraudStatus), value)
					? (FraudStatus)value
					: FraudStatus.UnderInvestigation;
			} // set
		} // FraudStatusValue

		public FraudStatus FraudStatus { get; private set; }

		public bool IsEmailSendingBanned {
			get { return !EmailSendingBanned.HasValue || EmailSendingBanned.Value; } // get
		} // IsEmailSendingBanned

		public int OfferLength {
			get {
				if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
					return 0;

				return (int)(OfferValidUntil.Value - OfferStart.Value).TotalHours;
			} // get
		} // OfferLength

		public List<string> ValidationErrors { get; private set; }

		public int? LacrAge {
			get {
				if (!LacrTime.HasValue)
					return null;

				return (int)(DateTime.UtcNow - LacrTime.Value).TotalDays;
			} // get
		} // LacrAge

		public decimal ApprovedAmount {
			get { return ManagerApprovedSum - TakenLoanAmount + RepaidPrincipal; } // get
		} // ApprovedAmount

		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");

			if (LacrID == 0)
				ValidationErrors.Add("no last manually approved cash request found");

			if (!LacrTime.HasValue)
				ValidationErrors.Add("last approved cash request time not filled");
		} // Validate

		public bool LacrIsTooOld(int nAge) {
			return !LacrAge.HasValue || LacrAge.Value > nAge;
		} // LacrIsTooOld
	} // class MetaData
} // namespace
