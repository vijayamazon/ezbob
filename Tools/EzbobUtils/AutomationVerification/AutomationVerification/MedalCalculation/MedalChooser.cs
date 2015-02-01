namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	///     Determines which medal to calculate for specified customer, calculates it and the offered amount.
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
		} // constructor

		/// <summary>
		///     Determines which medal to calculate and calculates it.
		///     Which medal type to choose logic:
		///     https://drive.google.com/open?id=0B1Io_qu9i44SVzVqV19nbnMxRW8&amp;authuser=0
		/// </summary>
		/// <param name="customerId">Customer ID.</param>
		/// <param name="calculationDate">
		///     Optional. Leave empty to calculate based on current data
		///     or specify date to calculate medal based on data available on that date.
		/// </param>
		/// <returns>Calculated medal model.</returns>
		public MedalOutputModel GetMedal(int customerId, DateTime? calculationDate = null) {
			var medalChooserData = DB.FillFirst<MedalChooserInputModelDb>(
				"AV_GetMedalChooserInputParams",
				new QueryParameter("@CustomerId", customerId)
			);

			DateTime today = calculationDate ?? DateTime.Today;

			bool hmrcTooOld = AccountIsTooOld(
				today,
				medalChooserData.HasHmrc,
				medalChooserData.LastHmrcUpdateDate,
				medalChooserData.MedalDaysOfMpRelevancy
			);

			if (hmrcTooOld) {
				return new MedalOutputModel {
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = "Hmrc data is too old",
					CustomerId = customerId,
				};
			} // if

			bool bankTooOld = AccountIsTooOld(
				today,
				medalChooserData.HasBank,
				medalChooserData.LastBankUpdateDate,
				medalChooserData.MedalDaysOfMpRelevancy
			);

			if (bankTooOld) {
				return new MedalOutputModel {
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = "Bank data is too old",
					CustomerId = customerId,
				};
			} // if

			var type = MedalType.NoMedal;

			if (medalChooserData.IsLimited)
				type = medalChooserData.HasOnline ? MedalType.OnlineLimited : MedalType.Limited;
			else if (medalChooserData.HasCompanyScore)
				type = medalChooserData.HasOnline ? MedalType.OnlineNonLimitedWithBusinessScore : MedalType.NonLimited;
			else if (medalChooserData.HasOnline)
				type = MedalType.OnlineNonLimitedNoBusinessScore;

			bool isSoleTrader = type == MedalType.NoMedal &&
				medalChooserData.HasPersonalScore &&
				(medalChooserData.HasBank || medalChooserData.HasHmrc);

			if (isSoleTrader)
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
					CustomerId = customerId
				};
			} // switch

			MedalInputModel data = medalCalulator.GetInputParameters(customerId, calculationDate);

			MedalOutputModel medal = medalCalulator.CalculateMedal(data);

			medal.OfferedLoanAmount = GetOfferedAmount(medal, medalChooserData.MinApprovalAmount);

			return medal;
		} // GetMeal

		protected AConnection DB;
		protected ASafeLog Log;

		private int GetOfferedAmount(MedalOutputModel medal, int minApprovalAmount) {
			var medalCoefficients = new List<MedalCoefficientsModelDb>();

			DB.ForEachRowSafe(sr => {
				Medal medalValue = (Medal)Enum.Parse(typeof(Medal), sr["Medal"]);

				medalCoefficients.Add(new MedalCoefficientsModelDb {
					Medal = medalValue,
					AnnualTurnover = sr["AnnualTurnover"],
					ValueAdded = sr["ValueAdded"],
					FreeCashFlow = sr["FreeCashFlow"],
				});
			}, "SELECT * FROM MedalCoefficients", CommandSpecies.Text);

			MedalCoefficientsModelDb coefficients = medalCoefficients.First(x => x.Medal == medal.Medal);

			decimal annualTurnoverOffer = decimal.Parse(medal.WeightsDict[Parameter.AnnualTurnover].Value) * coefficients.AnnualTurnover / 100.0M;
			decimal freeCashflowOffer = medal.UseHmrc ? medal.FreeCashflow * coefficients.FreeCashFlow / 100.0M : 0;
			decimal valueAddedOffer = medal.UseHmrc ? medal.ValueAdded * coefficients.ValueAdded / 100.0M : 0;

			decimal[] offers = { annualTurnoverOffer, freeCashflowOffer, valueAddedOffer, };

			List<decimal> positiveOffers = offers.Where(x => x >= minApprovalAmount).ToList();

			Log.Debug("Secondary medal - all   offer amounts: {0}", string.Join(", ", offers));
			Log.Debug("Secondary medal - valid offer amounts: {0}", string.Join(", ", positiveOffers));

			if (positiveOffers.Any()) {
				int theOffer = positiveOffers.Min(x => (int)x);
				Log.Debug("Secondary medal - offered loan amount calculated to be {0}.", theOffer);
				return theOffer;
			} // if

			Log.Debug("Secondary medal - all the offer amounts are not valid.");
			return 0;
		} // GetOfferedAmount

		private static bool AccountIsTooOld(DateTime today, bool hasAccounts, DateTime? lastUpdated, int threshold) {
			if (!hasAccounts)
				return false;

			if (lastUpdated == null)
				return true;

			return (today - lastUpdated.Value).TotalDays > threshold;
		} // AccountIsTooOld
	} // class MedalChooser
} // namespace
