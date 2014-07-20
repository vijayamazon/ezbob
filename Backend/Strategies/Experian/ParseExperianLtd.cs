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
			Result = new ExperianLtd();

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

			var oTbl = Save(Parse(Load()));

			if (oTbl != null)
				Result = oTbl;

			Log.Info("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		#endregion method Execute

		#region property Result

		public ExperianLtd Result { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		private readonly long m_nServiceLogID;

		#region method Load

		private Tuple<XmlDocument, DateTime> Load() {
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

			return new Tuple<XmlDocument, DateTime>(oXml, sr["InsertDate"]);
		} // Load

		#endregion method Load

		#region method Parse

		private ExperianLtd Parse(Tuple<XmlDocument, DateTime> oDoc) {
			if (oDoc == null)
				return null;

			if ((oDoc.Item1 == null) || (oDoc.Item1.DocumentElement == null))
				return null;

			Log.Info("Parsing Experian company data...");

			ExperianLtd oMainTable = new ExperianLtd(Log);
			oMainTable.LoadFromXml(oDoc.Item1.DocumentElement);

			if (!oMainTable.ShouldBeSaved()) {
				Log.Warn(
					"Parsing Experian company data failed: no main company data loaded for service log entry {0}.",
					m_nServiceLogID
				);

				return null;
			} // if

			oMainTable.ServiceLogID = m_nServiceLogID;

			Log.Info("Parsing Experian company data complete.");

			oMainTable.ReceivedTime = oDoc.Item2;

			return oMainTable;
		} // Parse

		#endregion method ParseAndSave

		#region method Save

		private ExperianLtd Save(ExperianLtd oMainTable) {
			if (oMainTable == null)
				return null;

			Log.Info("Saving Experian company data into DB...");

			ConnectionWrapper oPersistent = DB.GetPersistent();

			oPersistent.BeginTransaction();

			if (!oMainTable.Save(DB, oPersistent)) {
				oPersistent.Rollback();
				Log.Warn("Saving Experian company data into DB failed.");
				return null;
			} // if

			oPersistent.Commit();
			Log.Info("Saving Experian company data into DB complete.");
			return oMainTable;
		} // Save

		#endregion method Save

		#endregion private
	} // class ParseExperianLtd
} // namespace
