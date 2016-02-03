namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using MedalClass = EZBob.DatabaseLib.Model.Database.Medal;

	class BackdoorSimpleApprove : ABackdoorSimpleDetails {
		public static BackdoorSimpleApprove Create(string backdoorCode, int customerID, bool ownsProperty) {
			var match = regex.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug("Back door code '{0}' ain't no matches approval regex /{1}/.", backdoorCode, regex);
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
				ownsProperty
			);
		} // Create

		public MedalClass MedalClassification { get; private set; }

		public int ApprovedAmount { get; private set; }

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: approving...");

			response.ProposedAmount = ApprovedAmount;
			response.ApprovedAmount = ApprovedAmount;

			SafeReader sr = Library.Instance.DB.GetFirst(
				"GetDefaultLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty) {
				Log.Warn(
					"Back door simple approval failed for customer '{0}': could not load default loan source.",
					this.customerID
				);
				return false;
			} // if

			int sourceID = sr["LoanSourceID"];
			int repaymentPeriod = sr["RepaymentPeriod"];

			var offerDualCalculator = new OfferDualCalculator(
				this.customerID,
				DateTime.UtcNow,
				ApprovedAmount,
				false,
				MedalClassification,
				sourceID,
				repaymentPeriod
			);

			OfferResult offerResult = offerDualCalculator.CalculateOffer();

			if (offerResult == null || offerResult.IsError) {
				Log.Warn(
					"Back door simple for customer '{0}' - approval failed. Offer result: '{1}'.",
					this.customerID,
					offerResult != null ? offerResult.Description : string.Empty
				);
				return false;
			} // if

			response.HasApprovalChance = true;
			response.CreditResult = CreditResultStatus.Approved;
			response.UserStatus = Status.Approved;
			response.SystemDecision = SystemDecision.Approve;

			response.DecisionName = "Approval";
			response.AppValidFor = DateTime.UtcNow.AddHours(CurrentValues.Instance.OfferValidForHours);
			response.Decision = DecisionActions.Approve;
			response.LoanOfferEmailSendingBannedNew = false;

			response.RepaymentPeriod = offerResult.Period;
			response.LoanSourceID = sourceID;
			response.LoanTypeID = offerResult.LoanTypeId;
			response.InterestRate = offerResult.InterestRate / 100;
			response.SetupFee = offerResult.SetupFee / 100;

			DoDelay();

			Log.Debug("Back door simple flow: approved."); 

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
			bool ownsProperty
		) : base(DecisionActions.Approve, delay) {
			this.customerID = customerID;
			MedalClassification = medalClass;
			this.ownsProperty = ownsProperty;

			int appAmount;

			if (int.TryParse(approvedAmount, out appAmount))
				ApprovedAmount = MedalResult.RoundOfferedAmount(Cap(appAmount * 1000));
			else {
				var rnd = new Random();
				ApprovedAmount = MedalResult.RoundOfferedAmount(Cap(rnd.Next(CurrentValues.Instance.MinLoan, Pac)));
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

		private readonly int customerID;
		private readonly bool ownsProperty;
		private static readonly Regex regex = new Regex(@"^bds-a([fs])([sgpd])(\d{0,3})$");
	} // class BackdoorSimpleApprove
} // namespace
