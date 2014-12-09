namespace EzServiceConfigurationLoader {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EzServiceConfiguration;
	using Ezbob.Utils;
	using Newtonsoft.Json;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class QuickOfferConfiguration : EzServiceConfiguration.QuickOfferConfigurationData {

		public static QuickOfferEnabledStatus GetEnabledStatus(int nEnabled) {
			return Enum.GetValues(typeof (QuickOfferEnabledStatus)).Cast<int>().Any(x => x == nEnabled)
				? (QuickOfferEnabledStatus)nEnabled
				: QuickOfferEnabledStatus.Disabled;
		} // GetEnabledStatus

		public QuickOfferConfiguration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
			m_bIsLoaded = false;
			m_oOfferAmountPct = new List<Tuple<int, decimal>>();
			m_oPriceCalculator = new SortedTable<int, int, decimal>();
		} // constructor

		public override decimal OfferAmountPct(int nBusinessScore) {
			foreach (var oPct in m_oOfferAmountPct)
				if (nBusinessScore <= oPct.Item1)
					return oPct.Item2;

			return 0;
		} // OfferAmountPct

		public override decimal LoanPct(int nBusinessScore, decimal nRequestedAmount) {
			var oScores = m_oPriceCalculator.ColumnKeys;

			if (nBusinessScore >= oScores.Max)
				return 0.0m;

			var oAmounts = m_oPriceCalculator.RowKeys;

			if (nRequestedAmount >= oAmounts.Max)
				return 0.0m;

			var nScore = oScores.Min;
			foreach (var n in oScores) {
				if (nBusinessScore < n) {
					nScore = n;
					break;
				} // if
			} // for each score

			var nAmount = oAmounts.Min;
			foreach (var n in oAmounts) {
				if (nRequestedAmount < n) {
					nAmount = n;
					break;
				} // if
			} // for each score

			return m_oPriceCalculator[nAmount, nScore];
		} // LoanPct

		protected override void LoadFromDB() {
			m_oLog.Debug("Loading quick offer configuration from DB...");

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_bIsLoaded = true;

					Enabled = GetEnabledStatus((int)sr["Enabled"]);
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

		protected override void Adjust() {
			m_oOfferAmountPct.Clear();

			SortedDictionary<int, decimal> oap = JsonConvert.DeserializeObject<SortedDictionary<int, decimal>>(OfferAmountCalculator);

			foreach (KeyValuePair<int, decimal> pair in oap)
				m_oOfferAmountPct.Add(new Tuple<int, decimal>(pair.Key, pair.Value));

			if (m_oOfferAmountPct.Count < 1)
				throw new Exception(InvalidExceptionMessage, new Exception("Offer amount calculator table is empty."));

			m_oPriceCalculator.Clear();

			SortedDictionary<int, SortedDictionary<int, decimal>> pc = JsonConvert.DeserializeObject<SortedDictionary<int, SortedDictionary<int, decimal>>>(PriceCalculator);

			foreach (KeyValuePair<int, SortedDictionary<int, decimal>> pair in pc) {
				foreach (KeyValuePair<int, decimal> cell in pair.Value)
					m_oPriceCalculator[cell.Key, pair.Key] = cell.Value;
			} // for each row

			if (m_oPriceCalculator.IsEmpty)
				throw new Exception(InvalidExceptionMessage, new Exception("Price calculator table is empty."));
		} // Adjust

		protected override bool IsValid() {
			return m_bIsLoaded && base.IsValid();
		} // IsValid

		protected override void WriteToLog() {
			m_oLog.Debug("Quick offer configuration:");

			var ci = new CultureInfo("en-GB", false);

			m_oLog.Debug("Enabled: {0}.", Enabled);
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

			LogOfferAmountPct();
			LogPriceCalculator();

			m_oLog.Debug("End of quick offer configuration.");
		} // WriteToLog

		private void LogPriceCalculator() {
			var ci = new CultureInfo("en-GB", false);

			string sOutput = Environment.NewLine + m_oPriceCalculator.ToFormattedString(
				"Requested amount / Business score",
				oRowKeyToString: x => x.ToString("C0", ci),
				oColumnKeyToString: x => x.ToString("N0", ci),
				oDataToString: x => x.ToString("P2", ci)
			) + Environment.NewLine;

			m_oLog.Debug("Price calculator:\n{0}", sOutput);
		} // LogPriceCalculator

		private void LogOfferAmountPct() {
			var oKeys = new List<string>();
			var oLines = new List<string>();
			var oValues = new List<string>();

			const string sKeysTitle = " Business score";
			const string sValuesTitle = " % of total current assets";

			int nLength = Math.Max(sKeysTitle.Length, sValuesTitle.Length);

			string sFormat = string.Format("{{0,{0}}}", nLength);

			oKeys.Add(string.Format(sFormat, sKeysTitle));
			oLines.Add(new string('-', nLength));
			oValues.Add(string.Format(sFormat, sValuesTitle));

			foreach (Tuple<int, decimal> tpl in m_oOfferAmountPct) {
				string sKey = tpl.Item1.ToString("N0");
				string sValue = tpl.Item2.ToString("P2");

				nLength = Math.Max(sKey.Length, sValue.Length);

				sFormat = string.Format("{{0,{0}}}", nLength);

				oKeys.Add(string.Format(sFormat, sKey));
				oLines.Add(new string('-', nLength));
				oValues.Add(string.Format(sFormat, sValue));
			} // foreach

			string sOutput = Environment.NewLine +
				string.Join(" | ", oKeys) + Environment.NewLine +
				string.Join("-+-", oLines) + '-' + Environment.NewLine +
				string.Join(" | ", oValues) + Environment.NewLine;

			m_oLog.Debug("Offer amount calculator:\n{0}", sOutput);
		} // LogOfferAmountPct

		private readonly AConnection m_oDB;
		private readonly SafeLog m_oLog;
		private bool m_bIsLoaded;
		private readonly List<Tuple<int, decimal>> m_oOfferAmountPct;

		/// <summary>
		/// Row keys: money, requested amount.
		/// Column keys: int, business score.
		/// </summary>
		private readonly SortedTable<int, int, decimal> m_oPriceCalculator;

	} // class Configuration
} // namespace EzServiceConfigurationLoader
