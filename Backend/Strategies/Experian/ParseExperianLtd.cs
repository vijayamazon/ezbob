namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Text.RegularExpressions;
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

		private Tuple<XmlDocument, DateTime, string> Load() {

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

			return new Tuple<XmlDocument, DateTime, string>(oXml, sr["InsertDate"], sr["CompanyRefNum"]);
		} // Load

		private ExperianLtd Parse(Tuple<XmlDocument, DateTime, string> oDoc) {
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
					"Parsing Experian company data failed: no main company data loaded for service log entry {0}, using requested company ref num '{1}'.",
					m_nServiceLogID, oDoc.Item3
				);

				oMainTable.RegisteredNumber = oDoc.Item3;
			} // if

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

		#region method CleanInvalidChars

		// Source of this code: http://stackoverflow.com/questions/730133/invalid-characters-in-xml
		private static string CleanInvalidChars(string sXml) {
			// From XML spec valid chars: 
			// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
			// any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
			return Regex.Replace(sXml ?? string.Empty, @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]", string.Empty); 
		} // CleanInvalidChars

		#endregion method CleanInvalidChars

		#endregion private
	} // class ParseExperianLtd
} // namespace
