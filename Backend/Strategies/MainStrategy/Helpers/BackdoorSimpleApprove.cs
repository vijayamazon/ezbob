namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EZBob.DatabaseLib.Model.Database;
	using MedalClass = EZBob.DatabaseLib.Model.Database.Medal;

	class BackdoorSimpleApprove : ABackdoorSimpleDetails {
		public static BackdoorSimpleApprove Create(string backdoorCode, int customerID, bool ownsProperty) {
			return
				CreateWithGrade(backdoorCode, customerID, ownsProperty) ??
				CreateWithMedal(backdoorCode, customerID, ownsProperty);
		} // Create

		public MedalClass MedalClassification { get; private set; }

		public int ApprovedAmount { get; private set; }

		public decimal? GradeScore { get; private set; }
		public int? InvestorID { get; private set; }

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: approving customer {0}...", this.customerID);

			if (!CalculateMedalAndOffer(GradeScore, ApprovedAmount)) {
				Log.Debug(
					"Back door simple flow: not approved customer {0} because medal/offer calculation failed.",
					this.customerID
				);
				return false;
			} // if

			response.ProposedAmount = ApprovedAmount;
			response.ApprovedAmount = ApprovedAmount;
			response.HasApprovalChance = true;
			response.CreditResult = CreditResultStatus.Approved;
			response.UserStatus = Status.Approved;
			response.SystemDecision = SystemDecision.Approve;

			response.DecisionName = "Approval";
			response.AppValidFor = DateTime.UtcNow.AddHours(CurrentValues.Instance.OfferValidForHours);
			response.Decision = DecisionActions.Approve;
			response.LoanOfferEmailSendingBannedNew = false;

			response.RepaymentPeriod = OfferResult.Period;
			response.LoanSourceID = OfferResult.LoanSourceId;
			response.LoanTypeID = OfferResult.LoanTypeId;
			response.InterestRate = OfferResult.InterestRate / 100;
			response.SetupFee = OfferResult.SetupFee / 100;

			DoDelay();

			Log.Debug("Back door simple flow: approved for customer {0}.", this.customerID); 

			return true;
		} // SetResult

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"back door decision '{0}' after '{1}' seconds with medal '{2}' and approved amount of '{3}'.",
				Decision,
				Delay,
				MedalClassification,
				ApprovedAmount
			);
		} // ToString

		private BackdoorSimpleApprove(
			int customerID,
			int delay,
			MedalClass medalClass,
			string approvedAmount,
			bool ownsProperty,
			string gradeScore,
			string investorID
		) : base(customerID, DecisionActions.Approve, delay) {
			MedalClassification = medalClass;
			this.ownsProperty = ownsProperty;

			bool gradeMode = !string.IsNullOrWhiteSpace(gradeScore);

			int appAmount;

			if (int.TryParse(approvedAmount, out appAmount))
				ApprovedAmount = MedalResult.RoundOfferedAmount(gradeMode ? appAmount * 1000 : Cap(appAmount * 1000));
			else {
				var rnd = new Random();
				ApprovedAmount = MedalResult.RoundOfferedAmount(Cap(rnd.Next(CurrentValues.Instance.MinLoan, Pac)));
			} // if

			if (gradeMode) {
				GradeScore = decimal.Parse("0." + gradeScore);
				InvestorID = string.IsNullOrWhiteSpace(investorID) ? (int?)null : int.Parse(investorID.Substring(1));
			} else {
				GradeScore = null;
				InvestorID = null;
			} // if
		} // constructor

		private int Cap(int val) {
			return Math.Min(
				val,
				this.ownsProperty ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
			);
		} // Cap

		private int Pac {
			get {
				return 1 + (this.ownsProperty
					? CurrentValues.Instance.MaxCapHomeOwner
					: CurrentValues.Instance.MaxCapNotHomeOwner
				);
			} // get
		} // Pac

		private readonly bool ownsProperty;

		private static BackdoorSimpleApprove CreateWithGrade(string backdoorCode, int customerID, bool ownsProperty) {
			var match = regexGrade.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug(
					"Back door code '{0}' ain't no matches approval regex-with-grade /{1}/.",
					backdoorCode,
					regexGrade
				);
				return null;
			} // if

			return new BackdoorSimpleApprove(
				customerID,
				match.Groups[1].Value == "s" ? CurrentValues.Instance.WizardAutomationTimeout : 0,
				MedalClass.NoClassification,
				match.Groups[3].Value,
				ownsProperty,
				match.Groups[2].Value,
				match.Groups[4].Value
			);
		} // CreateWithGrade

		private static BackdoorSimpleApprove CreateWithMedal(string backdoorCode, int customerID, bool ownsProperty) {
			var match = regexMedal.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug(
					"Back door code '{0}' ain't no matches approval regex-with-medal /{1}/.",
					backdoorCode,
					regexMedal
				);
				return null;
			} // if

			MedalClass medal = MedalClass.NoClassification;

			switch (match.Groups[2].Value) {
			case "s":
				medal = MedalClass.Silver;
				break;

			case "g":
				medal = MedalClass.Gold;
				break;

			case "p":
				medal = MedalClass.Platinum;
				break;

			case "d":
				medal = MedalClass.Diamond;
				break;
			} // switch

			return new BackdoorSimpleApprove(
				customerID,
				match.Groups[1].Value == "s" ? CurrentValues.Instance.WizardAutomationTimeout : 0,
				medal,
				match.Groups[3].Value,
				ownsProperty,
				null,
				null
			);
		} // CreateWithMedal

		private static readonly Regex regexMedal = new Regex(@"^bds-a([fs])([sgpd])(\d{0,3})$");
		private static readonly Regex regexGrade = new Regex(@"^bds-a([fs])(\d{3})o(\d{0,3})(i\d+)?$");
	} // class BackdoorSimpleApprove
} // namespace
