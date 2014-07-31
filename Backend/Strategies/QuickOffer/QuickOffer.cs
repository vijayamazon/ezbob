namespace EzBob.Backend.Strategies.QuickOffer {
	using System;
	using System.Text.RegularExpressions;
	using EzServiceConfiguration;
	using EzServiceConfigurationLoader;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class QuickOffer : AStrategy {
		#region public

		#region method LoadConfiguration

		public static QuickOfferConfigurationData LoadConfiguration(AConnection oDB, ASafeLog oLog) {
			try {
				var qocfg = new QuickOfferConfiguration(oDB, oLog);
				qocfg.Init();
				return qocfg;
			}
			catch (Exception e) {
				new SafeLog(oLog).Alert(e, "Failed to load quick offer configuration.");
				return null;
			} // try
		} // LoadConfiguration

		#endregion method LoadConfiguration

		#region constructor

		public QuickOffer(int nCustomerID, bool bSaveOfferToDB, bool bHackForTest, QuickOfferConfigurationData oCfg, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_bSaveOfferToDB = bSaveOfferToDB;
			m_bHackForTest = bHackForTest;
			Offer = null;
			m_oCfg = oCfg ?? LoadConfiguration(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Quick offer"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			// * Except brokers' clients (source ref is liqcen).
			// * Limited Company only.
			// * Offline clients only.
			// * Min age: 18 years (BirthDate in Customer).
			// * Personal score >= 560 (ExperianConsumerScore in Customer).
			// * No defaults in last two years (ExperianDefaultAccount join CustomerId filter "Date").
			// + "Thick" file only.
			// * AML > 70 (AmlScore field in Customer table).
			// + The applicant should be director of the company (ExperianLtdDL72)
			// + Business score >= 31
			// * Company seniority: 3 years at least
			// + Tangible equity is positive

			// Starred items are currently (July 31, 2014) checked in the stored procedure: if all the conditions are met
			// it returns one row of data; otherwise it returns nothing.

			// Plussed items are checked in QuickOfferData.Load method.

			Log.Debug("QuickOffer.Execute started for customer {0}...", m_nCustomerID);

			if (ReferenceEquals(m_oCfg, null)) {
				Log.Debug("QuickOffer.Execute complete for customer {0}: configuration not specified.", m_nCustomerID);
				return;
			} // if

			if (m_oCfg.Enabled == QuickOfferEnabledStatus.Disabled) {
				Log.Debug("QuickOffer.Execute complete for customer {0}: quick offer is disabled.", m_nCustomerID);
				return;
			} // if

			if (m_bHackForTest)
				HackForTest();

			Offer = null;

			QuickOfferData qod = new QuickOfferData(m_oCfg, DB, Log);

			try {
				DB.ForEachRowSafe(
					(sr, bRowsetStart) => {
						qod.Load(sr);
						return ActionResult.SkipAll;
					},
					"QuickOfferDataLoad",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", m_nCustomerID)
				);
			}
			catch (Exception e) {
				Log.Alert("QuickOffer.Execute complete for customer {0}: failed to load quick offer calculation data from DB:\n\n{1}\n", m_nCustomerID, e.Message);
				return;
			} // try

			if (!qod.IsValid) {
				Log.Debug("QuickOffer.Execute complete for customer {0}: cannot make an offer based on data loaded from DB.", m_nCustomerID);
				return;
			} // if

			Offer = qod.GetOffer(m_bSaveOfferToDB, DB, Log);

			Log.Debug("QuickOffer.Execute complete for customer {0}: offer is {1}.", m_nCustomerID, ReferenceEquals(Offer, null) ? "alas, nothing" : Offer.Amount.ToString());
		} // Execute

		#endregion method Execute

		#region property Offer

		public QuickOfferModel Offer { get; private set; } // Offer

		#endregion property Offer

		#endregion public

		#region private

		#region method HackForTest

		private void HackForTest() {
			Log.Debug("QuickOffer.HackForTest for customer {0} started...", m_nCustomerID);

			string sEmail = string.Empty;

			try {
				DB.ForEachRowSafe(
					(sr, bRowsetStart) => {
						sEmail = sr["Email"];
						return ActionResult.SkipAll;
					},
					"LoadCustomerInfo",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", m_nCustomerID)
					);
			}
			catch (Exception e) {
				Log.Alert(e, "QuickOffer.HackForTest for customer {0} complete: failed to load customer email.", m_nCustomerID);
				return;
			} // try

			var match = Regex.Match(sEmail, @"\+qotest(\d+)@ezbob\.com$", RegexOptions.IgnoreCase);

			if (!match.Success) {
				Log.Debug("QuickOffer.HackForTest for customer {0} complete: customer email does not match the pattern.", m_nCustomerID);
				return;
			} // if

			int nTargetBusinessScore = 0;

			if (!int.TryParse(match.Groups[1].Captures[0].Value, out nTargetBusinessScore)) {
				Log.Debug("QuickOffer.HackForTest for customer {0} complete: failed to parse requested business score.", m_nCustomerID);
				return;
			} // if

			if (nTargetBusinessScore < m_oCfg.BusinessScoreMin) {
				Log.Debug("QuickOffer.HackForTest for customer {0} complete: requested business score is less than min business score.", m_nCustomerID);
				return;
			} // if

			try {
				DB.ExecuteNonQuery(
					"QuickOfferHackForTest",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", m_nCustomerID),
					new QueryParameter("@BusinessScore", nTargetBusinessScore)
				);
			}
			catch (Exception e) {
				Log.Alert("Failed to hack a customer for quick offer test: {0}", e.Message);
			} // try

			Log.Debug("QuickOffer.HackForTest for customer {0} complete.", m_nCustomerID);
		} // HackForTest

		#endregion method HackForTest

		private readonly int m_nCustomerID;
		private readonly bool m_bSaveOfferToDB;
		private readonly bool m_bHackForTest;
		private readonly QuickOfferConfigurationData m_oCfg;

		#endregion private
	} // class QuickOffer
} // namespace EzBob.Backend.Strategies.QuickOffer
