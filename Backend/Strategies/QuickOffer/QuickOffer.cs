namespace EzBob.Backend.Strategies.QuickOffer {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Models;

	#region class QuickOffer

	public class QuickOffer : AStrategy {
		#region public

		#region constructor

		public QuickOffer(int nCustomerID, bool bSaveOfferToDB, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_bSaveOfferToDB = bSaveOfferToDB;
			Offer = null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Quick offer"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			// *  1. Except brokers' clients (source ref is liqcen).
			// *  2. Limited Company only.
			// *  3. Offline clients only.
			// *  4. Min age: 18 years (BirthDate in the latest by LastUpdateDate MP_ExperianDataCache join by CustomerId and Name and Surname).
			// *  5. Personal score >= 560 (ExperianScore in the latest by LastUpdateDate MP_ExperianDataCache join by CustomerId and Name and Surname).
			// *  6. No defaults in last two years (ExperianDefaultAccount join CustomerId filter "Date").
			// +  7. AML > 70 (join by CustomerId, the latest by InsertDate, with ServiceType = 'AML A check' in MP_ServiceLog: innerText of from document element ./ProcessConfigResultsBlock/EIAResultBlock/AuthenticationIndex).
			// +  8. The applicant should be director of the company (Experian contains ./REQUEST/DL72/DIRFORENAME and ./REQUEST/DL72/DIRSURNAME).
			// +  9. Business score >= 31 (./REQUEST/DL76/RISKSCORE in MP_ExperianDataCache join by CompanyRefNum).
			// + 10. Company seniority: 3 years at least (./REQUEST/DL12/DATEINCORP in MP_ExperianDataCache join by CompanyRefNum).
			// + 11. Tangible equity is positive (from ./REQUEST/DL99/* in MP_ExperianDataCache join by CompanyRefNum).
			//       DL99 values should be from the latest financial year:
			//       DATEOFACCOUNTS-YYYY, DATEOFACCOUNTS-MM, DATEOFACCOUNTS-DD

			// Starred items are currently (Jan 22 2014) checked in the stored procedure: if all the conditions are met
			// it returns one row of data; otherwise it returns nothing.

			// Plussed items are checked in QuickOfferData.Load method.

			Offer = null;

			Log.Debug("QuickOffer.Execute started for customer {0}...", m_nCustomerID);

			var qod = new QuickOfferData(Log);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					qod.Load(sr);
					return ActionResult.SkipAll;
				},
				"QuickOfferDataLoad", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", m_nCustomerID)
			);

			if (!qod.IsValid) {
				Log.Debug("QuickOffer.Execute for customer {0} complete: cannot make an offer based on data loaded from DB.", m_nCustomerID);
				return;
			} // if

			Offer = qod.GetOffer(m_bSaveOfferToDB, DB, Log);

			Log.Debug("QuickOffer.Execute for customer {0} complete: offer is {1}.", m_nCustomerID, ReferenceEquals(Offer, null) ? "alas, nothing" : Offer.Amount.ToString());
		} // Execute

		#endregion method Execute

		#region property Offer

		public QuickOfferModel Offer { get; private set; } // Offer

		#endregion property Offer

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly bool m_bSaveOfferToDB;

		#endregion private
	} // class QuickOffer

	#endregion class QuickOffer
} // namespace EzBob.Backend.Strategies.QuickOffer
