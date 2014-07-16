namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class ParseExperianLtd : AStrategy {
		#region public

		#region constructor

		public ParseExperianLtd(long nServiceLogID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nServiceLogID = nServiceLogID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "ParseExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Info("Parsing Experian Ltd for service log entry {0}...", m_nServiceLogID);

			Save(Parse(Load()));

			Log.Info("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly long m_nServiceLogID;

		#region method Load

		private XmlDocument Load() {
			SafeReader sr = DB.GetFirst(
				"LoadServiceLogEntry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@EntryID", m_nServiceLogID)
			);

			if (sr["Id"] != m_nServiceLogID) {
				Log.Info("Parsing Experian Ltd for service log entry {0} failed: entry not found.", m_nServiceLogID);
				return null;
			} // if

			XmlDocument oXml = new XmlDocument();

			try {
				oXml.LoadXml(sr["ResponseData"]);
			}
			catch (Exception e) {
				Log.Alert(e, "Parsing Experian Ltd for service log entry {0} failed.", m_nServiceLogID);
				return null;
			} // try

			if (oXml.DocumentElement == null) {
				Log.Alert("Parsing Experian Ltd for service log entry {0} failed (no root element found).", m_nServiceLogID);
				return null;
			} // try

			return oXml;
		} // Load

		#endregion method Load

		#region method Parse

		private ExperianLtd Parse(XmlDocument oXml) {
			if ((oXml == null) || (oXml.DocumentElement == null))
				return null; // this should never happen, but to shut resharper up...

			Log.Info("Parsing Experian company data...");

			ExperianLtd oMainTable = new ExperianLtd(oXml.DocumentElement, Log);
			oMainTable.LoadFromXml();

			if (!oMainTable.ShouldBeSaved()) {
				Log.Warn(
					"Parsing Experian company data failed: no main company data loaded for service log entry {0}.",
					m_nServiceLogID
				);

				return null;
			} // if

			oMainTable.ServiceLogID = m_nServiceLogID;

			Log.Info("Parsing Experian company data complete.");

			return oMainTable;
		} // Parse

		#endregion method ParseAndSave

		#region method Save

		private void Save(ExperianLtd oMainTable) {
			if (oMainTable == null)
				return;

			Log.Info("Saving Experian company data into DB...");

			ConnectionWrapper oPersistent = DB.GetPersistent();

			oPersistent.BeginTransaction();

			if (!oMainTable.Save(DB, oPersistent)) {
				oPersistent.Rollback();
				Log.Warn("Saving Experian company data into DB failed.");
				return;
			} // if

			oPersistent.Commit();
			Log.Info("Saving Experian company data into DB complete.");
		} // Save

		#endregion method Save

		#endregion private
	} // class ParseExperianLtd
} // namespace
