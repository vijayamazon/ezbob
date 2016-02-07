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
		public MedalOutputModel GetMedal(int customerId, DateTime calculationDate) {
			var turnoverCalc = new TurnoverCalculator(customerId, calculationDate, DB, Log);

			string errorMsg;
			var type = turnoverCalc.GetMedalType(out errorMsg);

			if (!string.IsNullOrWhiteSpace(errorMsg)) {
				return new MedalOutputModel {
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					TurnoverType = null,
					Error = errorMsg,
					CustomerId = customerId,
				};
			} // if

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

			SetOfferedAmount(
				medal,
				turnoverCalc.MinApprovalAmount
			);

			return medal;
		} // GetMedal

		protected AConnection DB;
		protected ASafeLog Log;

		private void SetOfferedAmount(MedalOutputModel medal, int minApprovalAmount) {
			medal.OfferedLoanAmount = 0;
			medal.MaxOfferedLoanAmount = 0;

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

			decimal annualTurnoverOffer =
				decimal.Parse(medal.WeightsDict[Parameter.AnnualTurnover].Value) * coefficients.AnnualTurnover / 100.0M;
			decimal freeCashflowOffer = medal.UseHmrc ? medal.FreeCashflow * coefficients.FreeCashFlow / 100.0M : 0;
			decimal valueAddedOffer = medal.UseHmrc ? medal.ValueAdded * coefficients.ValueAdded / 100.0M : 0;

			decimal[] offers = { annualTurnoverOffer, freeCashflowOffer, valueAddedOffer, };

			List<decimal> positiveOffers = offers.Where(x => x >= minApprovalAmount).ToList();

			Log.Debug("Secondary medal - all   offer amounts: {0}", string.Join(", ", offers));
			Log.Debug("Secondary medal - valid offer amounts: {0}", string.Join(", ", positiveOffers));

			if (positiveOffers.Any()) {
				int thePreOffer = positiveOffers.Min(x => (int)x);
				int theMaxPreOffer = positiveOffers.Max(x => (int)x);

				medal.OfferedLoanAmount = (int)(thePreOffer * medal.CapOfferByCustomerScoresValue);
				medal.MaxOfferedLoanAmount = (int)(theMaxPreOffer * medal.CapOfferByCustomerScoresValue);

				Log.Debug(
					"Secondary medal - offered amount is {0}" +
					"(before score cap: {1}, cap value: {2}, consumer score: {3}, business score: {4}).",
					medal.OfferedLoanAmount,
					thePreOffer,
					medal.CapOfferByCustomerScoresValue.ToString("P6"),
					medal.ConsumerScore,
					medal.BusinessScore
				);

				Log.Debug(
					"Secondary medal - MAX offered loan amount is {0} (before score cap: {1}, cap value: {2}).",
					medal.MaxOfferedLoanAmount,
					theMaxPreOffer,
					medal.CapOfferByCustomerScoresValue.ToString("P6")
				);
			} else
				Log.Debug("Secondary medal - all the offer amounts are not valid.");
		} // GetOfferedAmount
	} // class MedalChooser
} // namespace
