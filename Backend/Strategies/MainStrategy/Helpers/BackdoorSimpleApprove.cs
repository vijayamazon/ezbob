namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using System.Text.RegularExpressions;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Database;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;
	using MedalClass = EZBob.DatabaseLib.Model.Database.Medal;

	class BackdoorSimpleApprove : ABackdoorSimpleDetails {
		public static ABackdoorSimpleDetails Create(
			string backdoorCode,
			int customerID,
			bool ownsProperty,
			decimal requestedAmount,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int delay
		) {
			return
				CreateWithGrade(
					backdoorCode,
					customerID,
					ownsProperty,
					requestedAmount,
					homeOwnerCap,
					notHomeOwnerCap,
					delay
				) ?? CreateWithMedal(
					backdoorCode,
					customerID,
					ownsProperty,
					requestedAmount,
					homeOwnerCap,
					notHomeOwnerCap,
					delay
				);
		} // Create

		public MedalClass MedalClassification { get; private set; }

		public decimal? GradeScore { get; private set; }
		public int? InvestorID { get; private set; }

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: approving customer {0}...", this.customerID);

			if (!CalculateMedalAndOffer()) {
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
			response.AppValidFor = DateTime.UtcNow.AddHours(this.offerValidHours);
			response.Decision = DecisionActions.Approve;
			response.LoanOfferEmailSendingBannedNew = false;

			response.RepaymentPeriod = OfferResult.Period;
			response.LoanSourceID = OfferResult.LoanSourceId;
			response.LoanTypeID = OfferResult.LoanTypeId;
			response.InterestRate = OfferResult.InterestRate / 100;
			response.SetupFee = OfferResult.SetupFee / 100;
			response.ProductSubTypeID = this.productSubTypeID;

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
			string score = GradeScore == null ? "N/A" : GradeScore.Value.ToString("N3");

			string investor = InvestorID == null
				? "do search"
				: (InvestorID <= 0 ? "leave pending" : "set id " + InvestorID.Value);

			return string.Format(
				"back door decision '{0}' after '{1}' seconds with medal '{2}', " +
				"approved amount of '{3}', grade score of '{4}', and investor '{5}'.",
				Decision,
				Delay,
				MedalClassification,
				ApprovedAmount,
				score,
				investor
			);
		} // ToString

		protected override bool CalculateMedalAndOffer() {
			if (GradeScore != null) {
				DB.ExecuteNonQuery(
					"BackdoorCreateGradeScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerID),
					new QueryParameter("@Score", GradeScore.Value),
					new QueryParameter("@MonthlyRepayment", this.requestedLoan.MonthlyPayment),
					new QueryParameter("@UniqueID", Guid.NewGuid()),
					new QueryParameter("@Now", DateTime.UtcNow)
				);
			} // if

			if (!base.CalculateMedalAndOffer())
				return false;

			OfferResult.IsError = true;

			decimal minInterestRate;
			decimal maxInterestRate;
			decimal minSetupFee;
			decimal maxSetupFee;
			int term;

			Medal.TotalScoreNormalized = 1;
			Medal.MedalClassification = MedalClassification;
			OfferResult.MedalClassification = MedalClassification;

			if (GradeScore == null) {
				OfferResult.FlowType = AutoDecisionFlowTypes.Internal;

				var loader = new LoadOfferRanges(ApprovedAmount, false, Medal.MedalClassification, DB, Log) {
					LoadLoanTypeID = false,
				}.Execute();

				if (!loader.Success) {
					Log.Warn(
						"Back door simple failed for customer '{0}': failed to load loan parameters ranges.",
						this.customerID
					);
					return false;
				} // if

				term = OfferResult.Period;
				minInterestRate = loader.InterestRate.Min;
				maxInterestRate = loader.InterestRate.Max;
				minSetupFee = loader.SetupFee.Min;
				maxSetupFee = loader.SetupFee.Max;
			} else {
				OfferResult.FlowType = AutoDecisionFlowTypes.LogicalGlue;

				var matchingGradeRanges = new MatchingGradeRanges();

				var spRanges = new LoadMatchingGradeRanges(DB, Log) {
					IsFirstLoan = true,
					IsRegulated = this.typeOfBusiness.IsRegulated(),
					LoanSourceID = OfferResult.LoanSourceId,
					OriginID = this.customerOriginID,
					Score = GradeScore.Value,
				};
				spRanges.Execute(matchingGradeRanges);

				if (matchingGradeRanges.Count != 1) {
					Log.Warn(
						"Back door simple failed for customer '{0}': {1} found.",
						this.customerID,
						Grammar.Number(matchingGradeRanges.Count, "matching grade range")
					);
					return false;
				} // if

				var gradeRangeID = matchingGradeRanges[0].GradeRangeID;
				this.productSubTypeID = matchingGradeRanges[0].ProductSubTypeID;

				SafeReader sr = DB.GetFirst(
					"LoadGradeRangeAndSubproduct",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@GradeRangeID", gradeRangeID),
					new QueryParameter("@ProductSubTypeID", this.productSubTypeID)
				);

				if (sr.IsEmpty) {
					Log.Warn(
						"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
						gradeRangeID,
						this.productSubTypeID
					);

					return false;
				} // if

				GradeRangeSubproduct grsp = sr.Fill<GradeRangeSubproduct>();

				OfferResult.GradeID = grsp.GradeID;
				OfferResult.SubGradeID = grsp.SubGradeID;

				OfferResult.Amount = grsp.LoanAmount(ApprovedAmount);

				term = grsp.Term(this.requestedLoan.RequestedTerm);
				minInterestRate = grsp.MinInterestRate;
				maxInterestRate = grsp.MaxInterestRate;
				minSetupFee = grsp.MinSetupFee;
				maxSetupFee = grsp.MaxInterestRate;
			} // if

			var calc = new OfferCalculator(
				OfferResult.FlowType,
				this.customerID,
				OfferResult.LoanSourceId,
				ApprovedAmount,
				term,
				false,
				this.aspireToMinSetupFee,
				this.smallLoanScenarioLimit,
				minInterestRate,
				maxInterestRate,
				minSetupFee,
				maxSetupFee,
				Log
			).Calculate();

			if (!calc.Success) {
				Log.Warn(
					"Back door simple failed for customer '{0}': calculator error.",
					this.customerID
				);

				return false;
			} // if

			OfferResult.Period = term;
			OfferResult.InterestRate = calc.InterestRate * 100.0M;
			OfferResult.SetupFee = calc.SetupFee * 100.0M;
			OfferResult.IsError = false;
			OfferResult.HasDecision = true;

			return true;
		} // CalcualteMedalAndOffer

		private BackdoorSimpleApprove(
			int homeOwnerCap,
			int notHomeOwnerCap,
			int customerID,
			int delay,
			MedalClass medalClass,
			bool ownsProperty,
			string gradeScore,
			string investorID,
			decimal requestedAmount
		) : base(
			homeOwnerCap,
			notHomeOwnerCap,
			customerID,
			DecisionActions.Approve,
			delay,
			ownsProperty,
			requestedAmount,
			!string.IsNullOrWhiteSpace(gradeScore)
		) {
			MedalClassification = medalClass;

			this.productSubTypeID = null;

			if (!string.IsNullOrWhiteSpace(gradeScore)) {
				GradeScore = decimal.Parse("0." + gradeScore);
				InvestorID = string.IsNullOrWhiteSpace(investorID) ? (int?)null : int.Parse(investorID.Substring(1));
			} else {
				GradeScore = null;
				InvestorID = null;
			} // if
		} // constructor

		private int? productSubTypeID;

		private static BackdoorSimpleApprove CreateWithGrade(
			string backdoorCode,
			int customerID,
			bool ownsProperty,
			decimal requestedAmount,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int delay
		) {
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
				homeOwnerCap,
				notHomeOwnerCap,
				customerID,
				match.Groups[1].Value == "s" ? delay : 0,
				MedalClass.NoClassification,
				ownsProperty,
				match.Groups[2].Value,
				match.Groups[3].Value,
				requestedAmount
			);
		} // CreateWithGrade

		private static BackdoorSimpleApprove CreateWithMedal(
			string backdoorCode,
			int customerID,
			bool ownsProperty,
			decimal requestedAmount,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int delay
		) {
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
				homeOwnerCap,
				notHomeOwnerCap,
				customerID,
				match.Groups[1].Value == "s" ? delay : 0,
				medal,
				ownsProperty,
				null,
				null,
				requestedAmount
			);
		} // CreateWithMedal

		private static readonly Regex regexMedal = new Regex(@"^bds-a([fs])([sgpd])$");
		private static readonly Regex regexGrade = new Regex(@"^bds-a([fs])(\d{3})(i\d+)?$");
	} // class BackdoorSimpleApprove
} // namespace
