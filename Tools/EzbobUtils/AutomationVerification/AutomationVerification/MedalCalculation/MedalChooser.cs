namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Linq;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	///     Determines which medal to calculate for specified customer, calculates it and the offered amount
	/// </summary>
	public class MedalChooser {
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="db">DB</param>
		/// <param name="log">Log</param>
		public MedalChooser(AConnection db, ASafeLog log) {
			this.DB = db;
			this.Log = log;
		}

		/// <summary>
		///     Determines which medal to calculate and calculated it
		///     rules are:
		///     a.	If customer has HMRC or bank that was last updated over 180 days ago (Config name = MedalDaysOfMpRelevancy) – No
		///     medal
		///     b.	If customer has more than 1 HMRC MP – No medal
		///     c.	If customer has limited company (LLP\Limited):
		///     i.		If customer has ebay or amazon or paypal  use "Online Limited Medal calculation logic"
		///     ii.		Else  use "Limited medal calculation logic"
		///     d.	Else if customer has company score (non-limited or non-LLP):
		///     i.		If customer has ebay or amazon or paypal   use "Online Non-Limited With Business Score Medal calculation
		///     logic"
		///     ii.		Else  use "Non-Limited Medal calculation logic"
		///     e.	Else if customer has ebay or amazon or paypal   use "Online Non-Limited No Business Score Medal calculation
		///     logic"
		///     f.	If no medal was selected so far and customer has all the following  use "Sole Trader Medal calculation logic"
		///     i.	Personal score
		///     ii.	HMRC or bank
		///     g.	If no medal was selected so far  no medal will be calculated for the customer
		/// </summary>
		/// <param name="customerId">CustomerId</param>
		/// <param name="calculationDate">optional if want to calculate medal based on historical data</param>
		/// <returns></returns>
		public MedalOutputModel GetMedal(int customerId, DateTime? calculationDate = null) {
			var dbHelper = new DbHelper(this.DB, this.Log);
			var medalChooserData = dbHelper.GetMedalChooserData(customerId);
			DateTime today = calculationDate.HasValue ? calculationDate.Value : DateTime.Today;

			if (medalChooserData.LastBankHmrcUpdateDate.HasValue &&
				(today - medalChooserData.LastBankHmrcUpdateDate.Value).TotalDays > medalChooserData.MedalDaysOfMpRelevancy) {
				return new MedalOutputModel {
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = "Bank or Hmrc data is too old",
					NumOfHmrcMps = medalChooserData.NumOfHmrc,
					CustomerId = customerId,
				};
			}

			if (medalChooserData.NumOfHmrc > 1) {
				return new MedalOutputModel {
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = string.Format("Customer has {0} HMRC MPs", medalChooserData.NumOfHmrc),
					NumOfHmrcMps = medalChooserData.NumOfHmrc,
					CustomerId = customerId,
				};
			}

			var type = MedalType.NoMedal;

			if (medalChooserData.IsLimited)
				type = medalChooserData.HasOnline ? MedalType.OnlineLimited : MedalType.Limited;
			else if (medalChooserData.HasCompanyScore)
				type = medalChooserData.HasOnline ? MedalType.OnlineNonLimitedWithBusinessScore : MedalType.NonLimited;
			else if (medalChooserData.HasOnline)
				type = MedalType.OnlineNonLimitedNoBusinessScore;

			if (
				type == MedalType.NoMedal &&
					medalChooserData.HasPersonalScore &&
					(medalChooserData.HasBank || medalChooserData.HasHmrc)
				)
				type = MedalType.SoleTrader;

			IMedalCalulator medalCalulator;
			switch (type) {
			case MedalType.Limited:
				medalCalulator = new OfflineLImitedMedalCalculator(this.DB, this.Log);
				break;
			case MedalType.NonLimited:
				medalCalulator = new NonLimitedMedalCalculator(this.DB, this.Log);
				break;
			case MedalType.OnlineLimited:
				medalCalulator = new OnlineLImitedMedalCalculator(this.DB, this.Log);
				break;
			case MedalType.SoleTrader:
				medalCalulator = new SoleTraderMedalCalculator(this.DB, this.Log);
				break;
			case MedalType.OnlineNonLimitedNoBusinessScore:
				medalCalulator = new OnlineNonLimitedNoBusinessScoreMedalCalculator(this.DB, this.Log);
				break;
			case MedalType.OnlineNonLimitedWithBusinessScore:
				medalCalulator = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.DB, this.Log);
				break;
			default:
				return new MedalOutputModel {
					MedalType = type,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = "None of the medals match the criteria for medal calculation",
					NumOfHmrcMps = medalChooserData.NumOfHmrc,
					CustomerId = customerId
				};
			}

			MedalInputModel data = medalCalulator.GetInputParameters(customerId, calculationDate);
			MedalOutputModel medal = medalCalulator.CalculateMedal(data);
			medal.OfferedLoanAmount = GetOfferedAmount(medal, medalChooserData.MinApprovalAmount);
			return medal;
		}

		protected AConnection DB;
		protected ASafeLog Log;

		private int GetOfferedAmount(MedalOutputModel medal, int minApprovalAmount) {
			var dbHelper = new DbHelper(this.DB, this.Log);
			var medalCoefficients = dbHelper.GetMedalCoefficients();
			var coefficients = medalCoefficients.First(x => x.Medal == medal.Medal);
			var annualTurnoverOffer = decimal.Parse(medal.WeightsDict[Parameter.AnnualTurnover].Value) *
				coefficients.AnnualTurnover / 100.0M;
			var freeCashflowOffer = medal.UseHmrc ? medal.FreeCashflow * coefficients.FreeCashFlow / 100.0M : 0;
			var valueAddedOffer = medal.UseHmrc ? medal.ValueAdded * coefficients.ValueAdded / 100.0M : 0;

			var offers = new[] {
				annualTurnoverOffer, freeCashflowOffer, valueAddedOffer
			};
			var positiveOffers = offers.Where(x => x >= minApprovalAmount)
				.ToList();
			return positiveOffers.Any() ? positiveOffers.Min(x => (int)x) : 0;
		}
	}
}
