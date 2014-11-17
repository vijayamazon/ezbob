namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	public class MetaData {
		#region constructor

		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		#endregion constructor

		#region properties read from DB

		[JsonIgnore]
		public string RowType { get; set; }

		public bool IsBrokerCustomer { get; set; }
		public int NumOfTodayAutoApproval { get; set; }
		public decimal TodayLoanSum { get; set; }

		[JsonIgnore]
		public int FraudStatusValue {
			get { return (int)FraudStatus; }
			set {
				FraudStatus = Enum.IsDefined(typeof (FraudStatus), value)
					? (FraudStatus)value
					: FraudStatus.UnderInvestigation;
			} // set
		} // FraudStatusValue

		public string AmlResult { get; set; }
		public string CustomerStatusName { get; set; }
		public bool CustomerStatusEnabled { get; set; }
		public int CompanyScore { get; set; }
		public int ConsumerScore { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime DateOfBirth { get; set; }

		public int NumOfDefaultAccounts { get; set; }
		public int NumOfRollovers { get; set; }

		public int TotalLoanCount { get; set; }
		public int OpenLoanCount { get; set; }
		public decimal TakenLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFees { get; set; }

		public DateTime? OfferValidUntil { get; set; }
		public DateTime? OfferStart { get; set; }
		public bool? EmailSendingBanned { get; set; }

		#endregion properties read from DB

		public FraudStatus FraudStatus { get; private set; }

		#region property IsEmailSendingBanned

		[JsonIgnore]
		public bool IsEmailSendingBanned {
			get { return !EmailSendingBanned.HasValue || EmailSendingBanned.Value; } // get
		} // IsEmailSendingBanned

		#endregion property IsEmailSendingBanned

		#region property OfferLength 

		[JsonIgnore]
		public double OfferLength {
			get {
				if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
					return 0;

				return (OfferValidUntil.Value - OfferStart.Value).TotalDays;
			} // get
		} // OfferLength

		#endregion property OfferLength

		public List<string> ValidationErrors { get; private set; }

		#region method Validate

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");

			// TODO

			if (!OfferStart.HasValue || !OfferValidUntil.HasValue)
				ValidationErrors.Add("last offer start time/length not filled");
		} // Validate

		#endregion method Validate

		#region property OutstandingPrincipal

		[JsonIgnore]
		public decimal OutstandingPrincipal {
			get {
				return TakenLoanAmount - RepaidPrincipal - SetupFees;
			} // get
		} // OutstandingPrincipal

		#endregion property OutstandingPrincipal

		#region property RepaidRatio

		[JsonIgnore]
		public decimal RepaidRatio {
			get {
				if (Math.Abs(TakenLoanAmount) < 0.00000001m)
					return 1;

				return (RepaidPrincipal + SetupFees) / TakenLoanAmount;
			} // get
		} // RepaidRatio

		#endregion property RepaidRatio
	} // class MetaData
} // namespace
