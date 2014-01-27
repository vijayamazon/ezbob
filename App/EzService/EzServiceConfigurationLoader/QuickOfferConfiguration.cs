using System;
using System.Data;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzServiceConfigurationLoader {
	using System.Globalization;

	public class QuickOfferConfiguration : EzServiceConfiguration.QuickOfferConfigurationData {
		#region public

		#region constructor

		public QuickOfferConfiguration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method LoadFromDB

		protected override void LoadFromDB() {
			m_oLog.Debug("Loading quick offer configuration from DB...");

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Enabled = sr["Enabled"];
					CompanySeniorityMonths = sr["CompanySeniorityMonths"];
					ApplicantMinAgeYears = sr["ApplicantMinAgeYears"];
					NoDefaultsInLastMonths = sr["NoDefaultsInLastMonths"];
					AmlMin = sr["AmlMin"];
					PersonalScoreMin = sr["PersonalScoreMin"];
					BusinessScoreMin = sr["BusinessScoreMin"];
					MaxLoanCountPerDay = sr["MaxLoanCountPerDay"];
					MaxIssuedValuePerDay = sr["MaxIssuedValuePerDay"];
					OfferDurationHours = sr["OfferDurationHours"];
					MinOfferAmount = sr["MinOfferAmount"];
					OfferCapPct = sr["OfferCapPct"];
					ImmediateMaxAmount = sr["ImmediateMaxAmount"];
					ImmediateTermMonths = sr["ImmediateTermMonths"];
					ImmediateInterestRate = sr["ImmediateInterestRate"];
					ImmediateSetupFee = sr["ImmediateSetupFee"];
					PotentialMaxAmount = sr["PotentialMaxAmount"];
					PotentialTermMonths = sr["PotentialTermMonths"];
					PotentialSetupFee = sr["PotentialSetupFee"];
					OfferAmountCalculator = sr["OfferAmountCalculator"];
					PriceCalculator = sr["PriceCalculator"];
					return ActionResult.SkipAll;
				},
				"QuickOfferLoadConfiguration",
				CommandSpecies.StoredProcedure
			);

			m_oLog.Debug("Loading quick offer configuration from DB complete.");
		} // LoadFromDB

		#endregion method LoadFromDB

		#region method Adjust

		protected override void Adjust() {
			// TODO
		} // Adjust

		#endregion method Adjust

		#region method WriteToLog

		protected override void WriteToLog() {
			m_oLog.Debug("Quick offer configuration:");

			var ci = new CultureInfo("en-GB", false);

			m_oLog.Debug("Enabled: {0}.", Enabled ? "yes" : "no");
			m_oLog.Debug("Company min seniority (months): {0}.", CompanySeniorityMonths);
			m_oLog.Debug("Applicant min age (years): {0}.", ApplicantMinAgeYears);
			m_oLog.Debug("No defaults in last {0} months.", NoDefaultsInLastMonths);
			m_oLog.Debug("AML at least: {0}.", AmlMin);
			m_oLog.Debug("Personal score at least: {0}.", PersonalScoreMin);
			m_oLog.Debug("Business score at least: {0}.", BusinessScoreMin);
			m_oLog.Debug("Max quick offer loan count per day: {0}.", MaxLoanCountPerDay);
			m_oLog.Debug("Max issued value in these loans: {0}.", MaxIssuedValuePerDay.ToString("C0", ci));
			m_oLog.Debug("Offer duration (hours): {0}.", OfferDurationHours);
			m_oLog.Debug("Min offer amount: {0}.", MinOfferAmount.ToString("C0", ci));
			m_oLog.Debug("Offer cap: {0} of the requested amount.", OfferCapPct.ToString("P2"));
			m_oLog.Debug("Max amount for an immediate offer: {0}.", ImmediateMaxAmount.ToString("C0", ci));
			m_oLog.Debug("Loan term for an immediate offer (months): {0}.", ImmediateTermMonths);
			m_oLog.Debug("Interest rate for an immediate offer: {0}.", ImmediateInterestRate.ToString("P2"));
			m_oLog.Debug("Setup fee for an immediate offer: {0} of the offer.", ImmediateSetupFee.ToString("P2"));
			m_oLog.Debug("Max amount for a potential offer: {0}.", PotentialMaxAmount.ToString("C0", ci));
			m_oLog.Debug("Loan term for a potential offer (months): {0}.", PotentialTermMonths);
			m_oLog.Debug("Setup fee for a potential offer: {0} of the offer.", PotentialSetupFee.ToString("P2"));

			// TODO

			m_oLog.Debug("End of quick offer configuration.");
		} // WriteToLog

		#endregion method WriteToLog

		#endregion protected

		#region private

		private readonly AConnection m_oDB;
		private readonly SafeLog m_oLog;

		#endregion private
	} // class Configuration
} // namespace EzServiceConfigurationLoader
