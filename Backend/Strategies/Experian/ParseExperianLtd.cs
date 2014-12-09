namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Text.RegularExpressions;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class ParseExperianLtd : AStrategy {
		public ParseExperianLtd(long nServiceLogID) {
			Result = new ExperianLtd();

			m_nServiceLogID = nServiceLogID;
		} // constructor

		public override string Name {
			get { return "ParseExperianLtd"; }
		} // Name

		public override void Execute() {
			Log.Info("Parsing Experian Ltd for service log entry {0}...", m_nServiceLogID);

			var oTbl = Save(Parse(Load()));

			if (oTbl != null)
				Result = oTbl;

			Log.Info("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		public ExperianLtd Result { get; private set; }

		private readonly long m_nServiceLogID;

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
				oXml.LoadXml(CleanInvalidChars(sr["ResponseData"]));
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

		private ExperianLtd Parse(Tuple<XmlDocument, DateTime> oDoc) {
			if (oDoc == null)
				return null;

			if ((oDoc.Item1 == null) || (oDoc.Item1.DocumentElement == null))
				return null;

			Log.Info("Parsing Experian company data...");

			ExperianLtd oMainTable = new ExperianLtd(Log) {
				ServiceLogID = m_nServiceLogID,
			};

			oMainTable.LoadFromXml(oDoc.Item1.DocumentElement);

			if (!oMainTable.ShouldBeSaved()) {
				Log.Warn(
					"Parsing Experian company data failed: no main company data loaded for service log entry {0}.",
					m_nServiceLogID
				);

				return null;
			} // if

			Log.Info("Parsing Experian company data complete.");

			oMainTable.ReceivedTime = oDoc.Item2;

			return oMainTable;
		} // Parse

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

		// Source of this code: http://stackoverflow.com/questions/730133/invalid-characters-in-xml
		private static string CleanInvalidChars(string sXml) {
			// From XML spec valid chars: 
			// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
			// any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
			return Regex.Replace(sXml ?? string.Empty, @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]", string.Empty);
		} // CleanInvalidChars

	} // class ParseExperianLtd
} // namespace
